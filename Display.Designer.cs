﻿namespace TechniteSimulation
{
	partial class Display
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.canvas = new System.Windows.Forms.Panel();
			this.doEvolve = new System.Windows.Forms.CheckBox();
			this.commits = new System.Windows.Forms.Label();
			this.offCommits = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Interval = 1;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// canvas
			// 
			this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.canvas.Location = new System.Drawing.Point(0, 45);
			this.canvas.Name = "canvas";
			this.canvas.Size = new System.Drawing.Size(283, 214);
			this.canvas.TabIndex = 1;
			this.canvas.Resize += new System.EventHandler(this.Display_ResizeEnd);
			// 
			// doEvolve
			// 
			this.doEvolve.AutoSize = true;
			this.doEvolve.Checked = true;
			this.doEvolve.CheckState = System.Windows.Forms.CheckState.Checked;
			this.doEvolve.Location = new System.Drawing.Point(0, 12);
			this.doEvolve.Name = "doEvolve";
			this.doEvolve.Size = new System.Drawing.Size(59, 17);
			this.doEvolve.TabIndex = 2;
			this.doEvolve.Text = "Evolve";
			this.doEvolve.UseVisualStyleBackColor = true;
			// 
			// commits
			// 
			this.commits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.commits.AutoSize = true;
			this.commits.Location = new System.Drawing.Point(177, 13);
			this.commits.Name = "commits";
			this.commits.Size = new System.Drawing.Size(106, 13);
			this.commits.TabIndex = 3;
			this.commits.Text = "Commits: 000000000";
			// 
			// offCommits
			// 
			this.offCommits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.offCommits.AutoSize = true;
			this.offCommits.Location = new System.Drawing.Point(229, 29);
			this.offCommits.Name = "offCommits";
			this.offCommits.Size = new System.Drawing.Size(54, 13);
			this.offCommits.TabIndex = 4;
			this.offCommits.Text = "/0000000";
			// 
			// Display
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.offCommits);
			this.Controls.Add(this.commits);
			this.Controls.Add(this.doEvolve);
			this.Controls.Add(this.canvas);
			this.Name = "Display";
			this.Text = "Display";
			this.ResizeEnd += new System.EventHandler(this.Display_ResizeEnd);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Panel canvas;
		private System.Windows.Forms.CheckBox doEvolve;
		private System.Windows.Forms.Label commits;
		private System.Windows.Forms.Label offCommits;
	}
}