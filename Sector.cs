using System;
using System.Collections.Generic;

namespace TechniteSimulation
{
	public class Sector
	{
		public struct ID
		{
			public readonly int X, Y;

			public ID(int x, int y)
			{
				X = x;
				Y = y;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 31 + X.GetHashCode();
					hash = hash * 31 + Y.GetHashCode();
					return hash;
				}
			}

			public override string ToString()
			{
				return X + ", "+Y;
			}

			public static bool operator==(ID a, ID b)
			{
				return a.X == b.X && a.Y == b.Y;
			}
			public static bool operator !=(ID a, ID b) { return !(a == b); }
			public override bool Equals(object obj)
			{
				return obj is ID && ((ID)obj) == this;
			}

			private static int Sqr(int x)
			{
				return x * x;
			}
			internal int QuadraticDistanceTo(ID other)
			{
				return Sqr(X - other.X) + Sqr(Y - other.Y);
			}
		}
		public readonly ID MyID;

		public Sector(ID id)
		{
			MyID = id;
			sequence = new Sequence(new State[] { new State(id, 0) },0,1);
		}
		//public NeighborAxis[] Neighbors = new NeighborAxis[2];

		public Dictionary<ID, Neighbor> neighbors = new Dictionary<ID, Neighbor>();

		public Sequence sequence;

		public class Neighbor
		{
			public readonly Sector Sector;
			public Sequence knownSequence;

			public Neighbor(Sector s)
			{
				Sector = s;
				knownSequence = null;
			}
		}
		public void Fetch(Random rng)
		{
			VisitNeighbors(delegate(Neighbor n)
			{
				if (rng.NextDouble() > 0.05)
					n.knownSequence = n.Sector.sequence;
			}
			);
		}


		internal struct NeighborAxis
		{
			public Neighbor Lower;
			public Neighbor Upper;

			public NeighborAxis(Sector sector1, Sector sector2)
			{
				this.Lower = new Neighbor(sector1);
				this.Upper = new Neighbor(sector2);
			}
		}

		public void VisitNeighbors(Action<Neighbor> act)
		{
			foreach (var n in neighbors.Values)
				act(n);
		}
		public IEnumerable<Tuple<ID, Sequence>> NeighborSequences()
		{
			foreach (var n in neighbors.Values)
				yield return new Tuple<ID, Sequence>(n.Sector.MyID,  n.knownSequence);
		}

		public int depth = 0;
		public int errors = 0;
		public void Update()
		{
			depth = sequence.MaxGeneration;
			sequence = sequence.Update(NeighborSequences(), ref depth, ref errors,sequence.States.Length);
			depth = sequence.MaxGeneration - depth;
		}

		public void Evolve()
		{
			depth = sequence.MaxGeneration+1;
			sequence = sequence.Evolve(NeighborSequences(),ref depth, ref errors);
			depth = sequence.MaxGeneration - depth;
		}

		public void Truncate(int maxDepth)
		{
			if (sequence.States.Length > maxDepth)
				sequence = sequence.Truncate(maxDepth);
		}

	}
}