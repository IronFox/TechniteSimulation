using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

		private Color GetColor(Sector sector)
		{
			int bad = 10 * sector.depth;

			int oldest = sector.commit.Generation - 1;
			if (sector.commit.otherParents != null)
				foreach (var n in sector.commit.otherParents)
				{
					oldest = Math.Min(oldest, n.Generation);
				}
			int age = sector.commit.Generation - oldest - 1;

			

			return Color.FromArgb(Math.Min(255, bad), Math.Min(255, age*10), Math.Min(255, sector.errors * 5));
		}
		public static int refreshCommitCount = 10;
		private void timer1_Tick(object sender, EventArgs e)
		{
			frame++;
			int at = 0;
			int step = canvas.Width / Program.tables.Length;
			foreach (var t in Program.tables)
			{
				t.Evolve(doEvolve.Checked, frame);
				PaintTable(t, new Rectangle(step * at, 0, step, canvas.Height));
				at++;
			}


			//		graphics.DrawEllipse(new Pen(Color.FromArgb(frame%256,0,0),10), rectangle);
			//graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);
		}

		private void PaintTable(SectorTable t, Rectangle rectangle)
		{
			
			float w = (float)rectangle.Width / t.sectors.GetLength(0);
			float h = (float)rectangle.Height / t.sectors.GetLength(1);

			for (int x = 0; x < t.sectors.GetLength(0); x++)
				for (int y = 0; y < t.sectors.GetLength(0); y++)
				{
					Sector s = t.sectors[x, y];
					graphics.FillRectangle(new SolidBrush(GetColor(s)), new RectangleF(x * w + rectangle.X, y * h + rectangle.Y, w, h));
				}

		}

		private void Display_ResizeEnd(object sender, EventArgs e)
		{
			graphics = canvas.CreateGraphics();
		}

	}
}
