﻿namespace SIL.FieldWorks.IText
{
	partial class StatisticsView
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
			this.statisticsBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			//
			// statisticsBox
			//
			this.statisticsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statisticsBox.Location = new System.Drawing.Point(0, 0);
			this.statisticsBox.Name = "statisticsBox";
			this.statisticsBox.ReadOnly = true;
			this.statisticsBox.Size = new System.Drawing.Size(522, 478);
			this.statisticsBox.TabIndex = 0;
			this.statisticsBox.Text = "";
			//
			// StatisticsView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.statisticsBox);
			this.Name = "StatisticsView";
			this.Size = new System.Drawing.Size(522, 478);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox statisticsBox;

	}
}