using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace discordify {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Form1._hookID = Form1.SetHook(Form1._proc);
			Application.Run(new Form1());
			Form1.UnhookWindowsHookEx(Form1._hookID);
		}
	}
}
