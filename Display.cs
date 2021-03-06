﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TechniteSimulation
{
	public partial class Display : Form
	{
		System.Drawing.Graphics graphics;
		public Display()
		{
			InitializeComponent();
			graphics = canvas.CreateGraphics();
			timer1.Start();
		}

		static int frame = 0;

		const int MaxHistoryLength = 64;

		private Color GetColor(Sector sector, bool colorErrors)
		{
			int bad = 0;// 255 * sector.depth / MaxHistoryLength;

			//int oldest = sector.sequence.Generation - 1;
			//if (sector.commit.otherParents != null)
			//	foreach (var n in sector.commit.otherParents)
			//	{
			//		oldest = Math.Min(oldest, n.Generation);
			//	}
			int age = (sector.sequence.States.Length - sector.sequence.ConsistentRange) * 255 / MaxHistoryLength;



			return Color.FromArgb(Math.Min(255, bad), Math.Min(255, age), colorErrors ? Math.Min(255, sector.errors*10) : 0);
		}
		public static int refreshCommitCount = 10;
		private void timer1_Tick(object sender, EventArgs e)
		{
			frame++;

			if (doEvolve.Checked)
				foreach (var t in Program.tables)
				{
					t.Evolve(false, frame, MaxHistoryLength, true, splitSystem.Checked,(float)errorLevel.Value / 100f);
				}
			foreach (var t in Program.tables)
				t.Evolve(doEvolve.Checked, frame, MaxHistoryLength,true, splitSystem.Checked, (float)errorLevel.Value / 100f);

			RepaintTables();
			commits.Text = Program.tables[0].sectors[0,0].sequence.MaxGeneration.ToString();
		}

		private void RepaintTables()
		{
			int at = 0;
			int step = canvas.Width / Program.tables.Length;
			foreach (var t in Program.tables)
			{
				PaintTable(t, new Rectangle(step * at, 0, step, canvas.Height));
				at++;
			}
		}

		private void PaintTable(SectorTable t, Rectangle rectangle)
		{
			Bitmap image = new Bitmap(t.sectors.GetLength(0), t.sectors.GetLength(1), PixelFormat.Format24bppRgb);


			var BoundsRect = new Rectangle(0, 0, image.Width, image.Height);
			BitmapData bmpData = image.LockBits(BoundsRect,
													ImageLockMode.WriteOnly,
													image.PixelFormat);

			IntPtr ptr = bmpData.Scan0;

			int bytes = bmpData.Stride * image.Height;
			var rgbValues = new byte[bytes];

			bool ce = colorErrors.Checked;
			for (int x = 0; x < t.sectors.GetLength(0); x++)
				for (int y = 0; y < t.sectors.GetLength(0); y++)
				{
					Sector s = t.sectors[x, y];
					Color c = GetColor(s, ce);
					rgbValues[x * 3 + y * bmpData.Stride+2] = c.R;
					rgbValues[x * 3 + y * bmpData.Stride+1] = c.G;
					rgbValues[x * 3 + y * bmpData.Stride] = c.B;
				}

			Marshal.Copy(rgbValues, 0, ptr, bytes);
			image.UnlockBits(bmpData);


			float w = (float)rectangle.Width / t.sectors.GetLength(0);
			float h = (float)rectangle.Height / t.sectors.GetLength(1);

			graphics.DrawImage(image, rectangle);


			for (int x = 0; x < t.sectors.GetLength(0); x++)
			{
				DrawSequence(t.sectors[x, 1].sequence,x,w,h);
			}


			//for (int x = 0; x < t.sectors.GetLength(0); x++)
			//	for (int y = 0; y < t.sectors.GetLength(0); y++)
			//	{
			//		graphics.FillRectangle(new SolidBrush(), new RectangleF(x * w + rectangle.X, y * h + rectangle.Y, w, h));
			//	}

		}

		private void DrawSequence(Sequence sequence, int x, float w, float h)
		{
			int d0 = 0,d1 = 0;
			for (int i = sequence.States.Length-1; i >= 0; i--)
			{
				ushort inc = sequence.States[i].InconsistentCells;
				if ((inc & 0x00f0) != 0)//right
					d1++;
				if ((inc & 0x0f00) != 0)//left
					d0++;
				if (inc == 0)
					break;
			}

			if (d0 == 0 && d1 == 0)
				return;
			graphics.DrawLine(p, (x) * w, d0 * h, (0.5f + x) * w, d0 * h);
			graphics.DrawLine(p, (0.5f+x) * w, d1 * h, (1f + x) * w, d1 * h);

	}

		Pen p = new Pen(Color.White);


		private void DrawLine(Pen p, Point offset, float w, float h, Sector.ID to, Sector.ID from)
		{
			graphics.DrawLine(p, from.X * w + offset.X, from.Y * h + offset.Y,
				to.X * w + offset.X, to.Y * h + offset.Y);
		}
		private void DrawLine(Pen p, Point offset, float w, float h, Sector.ID to, Sector.WeightedIDDelta from)
		{
			float x, y;
			from.Export(out x, out y);
			PointF p0 = new PointF((x + to.X) * w + offset.X, (y + to.Y) * h + offset.Y);
			PointF p1 = new PointF(to.X * w + offset.X, to.Y * h + offset.Y);
			//Pen p = new Pen(new System.Drawing.Drawing2D.LinearGradientBrush(p0, p1, Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 0, 0)));
			graphics.DrawLine(p, p0, p1);
			//p.Dispose();
		}

		private void Display_ResizeEnd(object sender, EventArgs e)
		{
			graphics = canvas.CreateGraphics();
		}

		private void flushErrors_Click(object sender, EventArgs e)
		{
			foreach (var t in Program.tables)
			{
				foreach (var s in t.sectors)
					s.errors = 0;
			}
		}

		private void simulate_CheckedChanged(object sender, EventArgs e)
		{
			timer1.Enabled = simulate.Checked;
		}

		private void canvas_Click(object sender, EventArgs e)
		{
			int x = MousePosition.X;// - canvas.Left - this.Left;
			int y = MousePosition.Y;// - canvas.Top - this.Top;



			int at = 0;
			int step = canvas.Width / Program.tables.Length;
			foreach (var t in Program.tables)
			{
				//t.Evolve(doEvolve.Checked, frame, MaxHistoryLength);
				Rectangle r = canvas.RectangleToScreen(
					new Rectangle(step * at, 0, step, canvas.Height) );
				at++;

				if (r.Contains(x,y))
				{
					int x2 = (x - r.Left) * t.sectors.GetLength(0) / r.Width;
					int y2 = (y - r.Top)* t.sectors.GetLength(1) / r.Height;

					t.sectors[x2, y2].ToggleIsolation();

					return;
				}
			}

		}

		private void drawGraph_CheckedChanged(object sender, EventArgs e)
		{
			if (!timer1.Enabled)
				RepaintTables();
		}
	}
}
