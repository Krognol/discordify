using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using spotify;
using discordrpc;

namespace discordify {
	internal enum PlayingStatus {
		Playing,
		Paused,
	}

	internal struct Status {
		public string CurArtist;
		public string CurTrack;
		public PlayingStatus PlayStatus;
		public int DurationMs;
		public DateTime Start;
		public DateTime PausedAt;
	}

	public partial class Form1 : Form {
		private static Form1 self;
		private static Spotify spotifyClient;

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		public static LowLevelKeyboardProc _proc = HookCallback;
		public static IntPtr _hookID = IntPtr.Zero;

		// Media virtual key codes
		private const int MEDIA_NEXT = 0xB0;
		private const int MEDIA_PREV = 0xB1;
		private const int MEDIA_PAPL = 0xB3; // Play/Pause

		private static string DISCORD_CLIENT_ID;
		private static string SPOTIFY_CLIENT_ID;
		private static string SPOTIFY_CLIENT_SECRET;

		private static DiscordRpc.RichPresence presence;
		private static DiscordRpc.EventHandlers handlers;
		private static Status curStatus;
		private static DateTime lastSwitch = DateTime.UtcNow;
		private static bool initialized = false;
		private bool quitting = false;

		// Used for other players
		private static List<string> processNames = new List<string>();
		private static string otherProcessSeparator;

		public Form1() {
			InitializeComponent();

			FormClosing += (object o, FormClosingEventArgs e) => {
				if (!quitting) {
					e.Cancel = true;
					Visible = false;
					ShowInTaskbar = false;
				}
			};

			var tray = new NotifyIcon {
				Text = "Discordify",
				Icon = new System.Drawing.Icon("Resources/tray_icon.ico")
			};
			var contexMenu = new ContextMenu();
			contexMenu.MenuItems.Add(new MenuItem("Show", (object o, EventArgs e) => {
				if (!Visible) {
					Visible = true;
					ShowInTaskbar = true;
				}
			}));
			contexMenu.MenuItems.Add(new MenuItem("Initialize", discordConnectButton_Click));
			contexMenu.MenuItems.Add(new MenuItem("Quit", discordQuitButton_Click));
			tray.ContextMenu = contexMenu;
			tray.Visible = true;

			DISCORD_CLIENT_ID = ConfigurationManager.AppSettings["discord_client_id"];
			SPOTIFY_CLIENT_ID = ConfigurationManager.AppSettings["spotify_client_id"];
			SPOTIFY_CLIENT_SECRET = ConfigurationManager.AppSettings["spotify_client_secret"];

			if (ConfigurationManager.AppSettings["other_process_name"] != null)
				processNames.Add(ConfigurationManager.AppSettings["other_process_name"]);
			otherProcessSeparator = (processNames.Count > 0 && ConfigurationManager.AppSettings["other_process_sep"] != null ?
				ConfigurationManager.AppSettings["other_process_sep"] : "");

			processNames.Add("Spotify");

			// Initialize the Spotify client
			spotifyClient = new Spotify(SPOTIFY_CLIENT_ID, SPOTIFY_CLIENT_SECRET);

			// This won't change if your application is set up properly 
			// so init them at the start.
			// Of course it's not necessary; if you don't have a spotify_large asset
			// Discord simply won't show anything but the details
			// however it looks kinda boring without it.
			// Of course you can do whatever, I'm not your boss
			presence.largeImageKey = "spotify_large";

			curStatus = new Status {
				CurArtist = "",
				CurTrack = "",
				PlayStatus = PlayingStatus.Paused
			};

			self = this;
		}

		private void updateStatus(string text) => toolStripStatusLabel2.Text = text;

		// DiscordRpc Ready callback
		private void Ready() => updateStatus("Ready!");

		// DiscordRpc Disconnected callback
		private void Disconnected(int errorCode, string message) => updateStatus(string.Format("Error {0}: {1}", errorCode, message));

		// DiscordRpc Error callback
		private void Error(int errorCode, string message) => updateStatus(string.Format("Error {0}: {1}", errorCode, message));

		// Connect button handler
		private void discordConnectButton_Click(object sender, EventArgs e) {
			if (initialized) return;

			if (DISCORD_CLIENT_ID == "") {
				updateStatus("Missing Discord client ID");
				return;
			}

			// Initialize the event handlers and DiscordRpc so we can begin
			// updating our rich presence
			handlers = new DiscordRpc.EventHandlers {
				readyCallback = Ready,
			};

			handlers.disconnectedCallback += Disconnected;
			handlers.errorCallback += Error;

			DiscordRpc.Initialize(DISCORD_CLIENT_ID, ref handlers, true, null);
			DiscordRpc.RunCallbacks();
			initialized = true;
			timer1.Enabled = true;
			updateStatus("Initialized!");
		}

		private async void UpdatePresence() {
			if (!initialized || curStatus.PlayStatus == PlayingStatus.Paused) return;

			if (SPOTIFY_CLIENT_ID == "" || SPOTIFY_CLIENT_SECRET == "") {
				updateStatus("Missing Spotify client ID or secret!");
				return;
			}

			string[] splitChar = new string[1];
			string track = "", artist = "";

			foreach (var name in processNames)
				using (var proc = Process.GetProcessesByName(name).First(p => !string.IsNullOrEmpty(p.MainWindowTitle))) {
					if (name != "Spotify") splitChar[0] = otherProcessSeparator;
					else splitChar[0] = " - ";
					var titleSplit = proc.MainWindowTitle.Split(splitChar, 2, StringSplitOptions.None);
					if (titleSplit.Length > 1) {
						artist = titleSplit[0];
						track = titleSplit[1];
					}
					break;
				}

			// Check so we're actually playing something
			if (track == "" && artist == "") return;

			// Check so the current artist and track aren't the same as
			if (curStatus.CurArtist == artist && curStatus.CurTrack == track) return;

			curStatus.CurArtist = artist;
			curStatus.CurTrack = track;

			// Search for the track via the spotify web api
			var spotifyStuff = await spotifyClient.SearchTrack(Uri.EscapeDataString($"artist:{artist} {track}"));
			Spotify.Tracks? tracks = null;
			if (spotifyStuff.HasValue) tracks = spotifyStuff?.Tracks;

			// Update the presence if the track was found
			if (tracks != null && tracks?.Items.Count > 0) {
				var item = tracks?.Items[0];
				presence.details = $"Listening to {item?.Name} by {item?.Artists[0].Name}";
				presence.smallImageKey = "spotify_small_playing";

				var now = DateTime.UtcNow;
				curStatus.DurationMs = (int)item?.DurationMs;
				curStatus.Start = now;
				presence.startTimestamp = DateTimeToTimestamp(now);
				presence.endTimestamp = DateTimeToTimestamp(now.AddMilliseconds((int)item?.DurationMs));

				DiscordRpc.UpdatePresence(ref presence);
				updateStatus(string.Format("Updated status: {0}", presence.details));
			}
		}

		private void discordQuitButton_Click(object sender, EventArgs e) {
			quitting = true;
			DiscordRpc.Shutdown();
			Application.Exit();
		}

		// Every 10 seconds we update the presence
		private void timer1_Tick(object sender, EventArgs e) => UpdatePresence();

		// Convert the Utc date time to a valid timestamp
		private static long DateTimeToTimestamp(DateTime dt) => (dt.Ticks - 621355968000000000) / 10000000;

		// Key intercepting stuff
		// Taken from https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/

		public static IntPtr SetHook(LowLevelKeyboardProc proc) {
			using (var curProc = Process.GetCurrentProcess())
			using (var curModule = curProc.MainModule) {
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
				var vkCode = Marshal.ReadInt32(lParam);
				switch (vkCode) {
				case MEDIA_PAPL:
					switch (curStatus.PlayStatus) {
					case PlayingStatus.Paused:
						curStatus.PlayStatus = PlayingStatus.Playing;

						// Set the start time from un-pausing to now
						var now = DateTime.UtcNow;
						presence.startTimestamp = DateTimeToTimestamp(now);

						// Get the elapsed time of the playing track
						// and update the end time
						var remaining = curStatus.Start
							.Subtract(curStatus.PausedAt)
							.Add(TimeSpan.FromMilliseconds(curStatus.DurationMs));
						presence.endTimestamp = DateTimeToTimestamp(now.Add(remaining));
						presence.smallImageKey = "spotify_small_playing";

						// Enable the timer again when un-pausing
						self.timer1.Enabled = true;

						DiscordRpc.UpdatePresence(ref presence);
						break;
					case PlayingStatus.Playing:
						curStatus.PlayStatus = PlayingStatus.Paused;
						curStatus.PausedAt = DateTime.UtcNow;

						presence.smallImageKey = "spotify_small_paused";
						presence.startTimestamp = 0;
						presence.endTimestamp = 0;

						// Don't check for updates while paused.
						self.timer1.Enabled = false;

						DiscordRpc.UpdatePresence(ref presence);
						break;
					}
					break;
				case MEDIA_NEXT:
				case MEDIA_PREV:
					if(curStatus.PlayStatus == PlayingStatus.Paused) {
						curStatus.PlayStatus = PlayingStatus.Playing;
						self.timer1.Enabled = true;
					}
					
					if (DateTime.UtcNow.Subtract(lastSwitch) >= TimeSpan.FromSeconds(5)) {
						lastSwitch = DateTime.UtcNow;
						self.UpdatePresence();
					}
					break;
				}
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
