namespace discordify {
	partial class Form1 {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.discordConnectButton = new System.Windows.Forms.Button();
			this.discordDisconnectButton = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.discordQuitButton = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2});
			this.statusStrip1.Location = new System.Drawing.Point(0, 51);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(434, 22);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 4;
			this.statusStrip1.Text = "ASdf";
			// 
			// toolStripStatusLabel2
			// 
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Size = new System.Drawing.Size(38, 17);
			this.toolStripStatusLabel2.Text = "Hello!";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
			// 
			// discordConnectButton
			// 
			this.discordConnectButton.Location = new System.Drawing.Point(12, 12);
			this.discordConnectButton.Name = "discordConnectButton";
			this.discordConnectButton.Size = new System.Drawing.Size(75, 23);
			this.discordConnectButton.TabIndex = 2;
			this.discordConnectButton.Text = "Connect";
			this.discordConnectButton.UseVisualStyleBackColor = true;
			this.discordConnectButton.Click += new System.EventHandler(this.discordConnectButton_Click);
			// 
			// discordDisconnectButton
			// 
			this.discordDisconnectButton.Location = new System.Drawing.Point(172, 12);
			this.discordDisconnectButton.Name = "discordDisconnectButton";
			this.discordDisconnectButton.Size = new System.Drawing.Size(75, 23);
			this.discordDisconnectButton.TabIndex = 3;
			this.discordDisconnectButton.Text = "Disconnect";
			this.discordDisconnectButton.UseVisualStyleBackColor = true;
			this.discordDisconnectButton.Click += new System.EventHandler(this.discordDisconnectButton_Click);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 10000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// discordQuitButton
			// 
			this.discordQuitButton.Location = new System.Drawing.Point(347, 12);
			this.discordQuitButton.Name = "discordQuitButton";
			this.discordQuitButton.Size = new System.Drawing.Size(75, 23);
			this.discordQuitButton.TabIndex = 5;
			this.discordQuitButton.Text = "Quit";
			this.discordQuitButton.UseVisualStyleBackColor = true;
			this.discordQuitButton.Click += new System.EventHandler(this.discordQuitButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 73);
			this.Controls.Add(this.discordQuitButton);
			this.Controls.Add(this.discordDisconnectButton);
			this.Controls.Add(this.discordConnectButton);
			this.Controls.Add(this.statusStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Discordify";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.Button discordConnectButton;
		private System.Windows.Forms.Button discordDisconnectButton;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button discordQuitButton;
	}
}

