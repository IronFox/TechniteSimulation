using System;
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

		const int MaxHistoryLength = 32;

		private Color GetColor(Sector sector, bool colorErrors)
		{
			int bad = 255 * sector.depth / MaxHistoryLength;

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
			int at = 0;
			int step = canvas.Width / Program.tables.Length;

			if (doEvolve.Checked)
				foreach (var t in Program.tables)
				{
					t.Evolve(false, frame, MaxHistoryLength, true);
				}
			foreach (var t in Program.tables)
			{
				t.Evolve(doEvolve.Checked, frame, MaxHistoryLength,true);
				PaintTable(t, new Rectangle(step * at, 0, step, canvas.Height));
				at++;
			}

			commits.Text = Program.tables[0].sectors[0,0].sequence.MaxGeneration.ToString();
			//		graphics.DrawEllipse(new Pen(Color.FromArgb(frame%256,0,0),10), rectangle);
			//graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);
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

			//for (int x = 0; x < t.sectors.GetLength(0); x++)
			//	for (int y = 0; y < t.sectors.GetLength(0); y++)
			//	{
			//		graphics.FillRectangle(new SolidBrush(), new RectangleF(x * w + rectangle.X, y * h + rectangle.Y, w, h));
			//	}

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
	}
}
