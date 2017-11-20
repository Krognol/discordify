using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections.Generic;
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
	}

	public partial class Form1 : Form {
		private static Spotify spotifyClient;

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		public static LowLevelKeyboardProc _proc = HookCallback;
		public static IntPtr _hookID = IntPtr.Zero;

		// Media virtual key codes
		// can probably remove the MUTE one
		private const int MEDIA_NEXT = 0xB0;
		private const int MEDIA_PREV = 0xB1;
		private const int MEDIA_PAPL = 0xB3;
		private const int MEDIA_MUTE = 0xAD;

		private string DISCORD_CLIENT_ID;
		private string SPOTIFY_CLIENT_ID;
		private string SPOTIFY_CLIENT_SECRET;

		private static DiscordRpc.RichPresence presence;
		private static DiscordRpc.EventHandlers handlers;
		private static Status curStatus;
		private static DateTime lastSwitch = DateTime.UtcNow;
		private static bool initialized = false;

		// Used for other music players
		private static List<string> otherProcessNames = new List<string>();
		private static List<string> otherProcessSeparator = new List<string>();

		public Form1() {
			InitializeComponent();

			FormClosing += (object o, FormClosingEventArgs e) => {
				e.Cancel = true;
				Visible = false;
				ShowInTaskbar = false;
			};

			var tray = new NotifyIcon {
				Text = "Discordify",
				Icon = new System.Drawing.Icon("Resources/tray_icon.ico")
			};
			var contexMenu = new ContextMenu();
			contexMenu.MenuItems.Add(new MenuItem("Connect", discordConnectButton_Click));
			contexMenu.MenuItems.Add(new MenuItem("Disconnect", discordDisconnectButton_Click));
			contexMenu.MenuItems.Add(new MenuItem("Quit", (object o, EventArgs e) => {
				DiscordRpc.Shutdown();
				Application.Exit();
			}));
			contexMenu.MenuItems.Add(new MenuItem("Show", (object o, EventArgs e) => {
				if (!Visible) {
					Visible = true;
					ShowInTaskbar = true;
				}
			}));
			tray.ContextMenu = contexMenu;
			tray.Visible = true;

			DISCORD_CLIENT_ID = ConfigurationManager.AppSettings["discord_client_id"];
			SPOTIFY_CLIENT_ID = ConfigurationManager.AppSettings["spotify_client_id"];
			SPOTIFY_CLIENT_SECRET = ConfigurationManager.AppSettings["spotify_client_secret"];

			if (ConfigurationManager.AppSettings["other_process_name"] != null)
				otherProcessNames.Add(ConfigurationManager.AppSettings["other_process_name"]);
			otherProcessSeparator.Add(otherProcessNames.Count > 0 && ConfigurationManager.AppSettings["other_process_sep"] != null ?
				ConfigurationManager.AppSettings["other_process_sep"] : "");

			otherProcessNames.Add("Spotify");

			// Initialize the Spotify client
			spotifyClient = new Spotify(SPOTIFY_CLIENT_ID, SPOTIFY_CLIENT_SECRET);

			// These won't change if your application is set up properly 
			// so init them at the start
			presence.largeImageKey = "spotify_large";

			curStatus = new Status {
				CurArtist = "",
				CurTrack = "",
				PlayStatus = PlayingStatus.Paused
			};
		}

		private void updateStatus(string text) => toolStripStatusLabel2.Text = text;

		// Call DiscordRpc.Shutdown when clicking disconnect button
		// This is probably redundant though since we call Shutdown
		// on FormClosing
		private void discordDisconnectButton_Click(object sender, EventArgs e) => DiscordRpc.Shutdown();

		// DiscordRpc Ready callback
		private void Ready() => updateStatus("Ready!");

		// DiscordRpc Disconnected callback
		private void Disconnected(int errorCode, string message) => updateStatus(string.Format("Error {0}: {1}", errorCode, message));

		// DiscordRpc Error callback
		private void Error(int errorCode, string message) => updateStatus(string.Format("Error {0}: {1}", errorCode, message));

		// Connect button handler
		private void discordConnectButton_Click(object sender, EventArgs e) {
			if (initialized) return;
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
		}

		private static async void UpdatePresence() {
			if (!initialized) return;

			string[] splitChar = { " - " };
			string track = "", artist = "";

			foreach (var name in otherProcessNames)
				using (var proc = (from p in Process.GetProcessesByName(name) where p.MainWindowTitle != "" select p).First()) {
					if (name != "Spotify") splitChar = otherProcessSeparator.ToArray();
					var titleSplit = proc.MainWindowTitle.Split(splitChar, 2, StringSplitOptions.None);
					if(titleSplit.Length > 1){
						artist = titleSplit[0];
						track = titleSplit[1];
					}
					break;
				}

			// Check so we're actually playing something
			if (track == "" || artist == "") return;

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

				var now = DateTime.UtcNow;
				presence.startTimestamp = DateTimeToTimestamp(now);
				presence.endTimestamp = DateTimeToTimestamp(now.AddMilliseconds((int)tracks?.Items[0].DurationMs));

				DiscordRpc.UpdatePresence(ref presence);
			}
		}

		private void discordQuitButton_Click(object sender, EventArgs e) {
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
						break;
					case PlayingStatus.Playing:
						curStatus.PlayStatus = PlayingStatus.Paused;
						break;
					}
					UpdatePresence();
					break;
				case MEDIA_NEXT:
				case MEDIA_PREV:
					if (DateTime.UtcNow.Subtract(lastSwitch) >= TimeSpan.FromSeconds(5)) {
						lastSwitch = DateTime.UtcNow;
						UpdatePresence();
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
