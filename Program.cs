using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TechniteSimulation
{
	class Program
	{
		public static SectorTable[] tables;
		[STAThread]
		static void Main(string[] args)
		{
			int size = 100;
			tables = new SectorTable[] {
				//new SectorTable(size, size,1),
				new SectorTable(size, size),
				//new SectorTable(size, size,2),

			};
			




			/*
			bool consistent = false;
			int iterations = 0;
			do
			{
				iterations++;

				foreach (Sector s in sectors)
					s.Fetch();
				foreach (Sector s in sectors)
					s.Update();


				consistent = true;
				foreach (Sector s in sectors)
					if (!s.IsConsistent)
					{
						consistent = false;
						break;
					}

			} while (!consistent);
			*/

			Application.EnableVisualStyles();
			Display wnd = new Display();
			wnd.Width = wnd.Height * tables.Length;
			Application.Run(wnd);
		}


	}
}
