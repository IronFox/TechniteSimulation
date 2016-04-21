using System;
using System.Linq;
using System.Threading.Tasks;

namespace TechniteSimulation
{
	public class SectorTable
	{
		public Sector[,] sectors;

		public SectorTable(int width, int height, float r0 = 1.5f)
		{
			sectors = new Sector[width, height];
			for (int i = 0; i < sectors.GetLength(0); i++)
				for (int j = 0; j < sectors.GetLength(1); j++)
				{
					sectors[i, j] = new Sector(new Sector.ID(i, j));
				}

			int radius = (int)Math.Ceiling(r0);
			float r2 = r0 *r0;
			for (int i = 0; i < sectors.GetLength(0); i++)
				for (int j = 0; j < sectors.GetLength(1); j++)
				{
					Sector s = sectors[i, j];
					for (int k = i - radius; k <= i + radius; k++)
						if (k >= 0 && k < sectors.GetLength(0))
							for (int l = j - radius; l <= j + radius; l++)
								if (l >= 0 && l < sectors.GetLength(1))
								{
									if (k != i || l != j)
										if (s.MyID.QuadraticDistanceTo(sectors[k, l].MyID) <= r2 )
										s.neighbors.Add(sectors[k, l].MyID, new Sector.Neighbor(sectors[k, l]));
								}
				}


		}

		Random rng = new Random(1024);


		internal void Evolve(bool doEvolve, int frame, int maxHistoryLength, bool optimize, bool split)
		{
			//Console.WriteLine(rng.Next());
			int splitAt = split ? sectors.GetLength(0) / 2 : -1;
			foreach (Sector s in sectors)
				s.Fetch(rng,frame,splitAt);
			Parallel.ForEach(sectors.Cast<Sector>(), (Sector s) =>
			{
				if (doEvolve) s.Evolve(optimize);
				else s.Update(optimize);
			});
			Parallel.ForEach(sectors.Cast<Sector>(), (Sector s) =>
			{
				s.Truncate(maxHistoryLength);
			});
		}
	}
}