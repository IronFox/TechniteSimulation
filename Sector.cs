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
				return X + ", " + Y;
			}

			public static bool operator ==(ID a, ID b)
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


			public static IDDelta operator -(ID a, ID b)
			{
				return new IDDelta(a.X - b.X, a.Y - b.Y);
			}
			public static ID operator+(ID a, IDDelta d)
			{
				return new ID(a.X + d.X, a.Y + d.Y);
			}

			static int xorshf96(int seed)
			{          //period 2^96-1
				int x = 123456789 * seed, y = 362436069 + seed, z = 521288629 ^ seed;
				int t;
				x ^= x << 16;
				x ^= x >> 5;
				x ^= x << 1;

				t = x;
				x = y;
				y = z;
				z = t ^ x ^ y;

				return z;
			}

			internal bool RelevantToEvolution(ID other, int generation)
			{

				//			val = val * 17 + generation;
				////			Random rng = new Random(val);
				//			float rnd = (float)(xorshf96(val)%0xFF) / 255.0f;
				//return true;
				//			int val = other.GetHashCode() * 17 + SectorID.GetHashCode();
				double rnd = PerlinNoise.Global.Noise(Math.PI *( X * 100 + (other.X - X) * 10), Math.E *(Y * 100 + (other.Y - Y) * 10), (double)generation / 20.0);
					//10.0 + 0.0 * (double)generation / 20.0f/* + val*/);
				return Math.Abs(rnd) < 0.05 / (QuadraticDistanceTo(other));
			}
		}


		public struct IDDelta
		{
			public readonly int X, Y;

			public IDDelta(int x, int y)
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
				return "+("+X + ", " + Y+")";
			}

			public static bool operator ==(IDDelta a, IDDelta b)
			{
				return a.X == b.X && a.Y == b.Y;
			}
			public static bool operator !=(IDDelta a, IDDelta b) { return !(a == b); }
			public override bool Equals(object obj)
			{
				return obj is IDDelta && ((IDDelta)obj) == this;
			}

			private static int Sqr(int x)
			{
				return x * x;
			}

			public static IDDelta operator*(IDDelta d, int f)
			{
				return new IDDelta(d.X * f, d.Y * f);
			}

			public static IDDelta operator+(IDDelta a, IDDelta b)
			{
				return new IDDelta(a.X + b.X, a.Y + b.Y);
			}
		}


		public struct WeightedIDDelta
		{
			IDDelta accumulated;
			int total;

			public bool IsNotEmpty { get { return total > 0; } }

			public void Add(IDDelta id, int weight)
			{
				accumulated += id * weight;
				total += weight;
			}
			public void Export(out float outX, out float outY)
			{
			 	outX = total > 0 ? (float)accumulated.X / total : 0f;
				outY = total > 0 ? (float)accumulated.Y / total : 0f;
			}
		}

		public readonly ID MyID;

		public Sector(ID id)
		{
			MyID = id;
			sequence = new Sequence(new State[] { new State(id, 0) },0,1,id,new WeightedIDDelta());
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

		public int isolated = 0;

		public void ToggleIsolation()
		{
			isolated += 100;

		}

		public void Fetch(Random rng, int frame)
		{
			if (isolated > 0)
			{
				isolated--;
				return;
			}
			VisitNeighbors(delegate(Neighbor n)
			{
				//if (rng.NextDouble() > 0.05)
				//if ((frame%3)==0)
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
		public void Update(bool optimize)
		{
			if (sequence.ConsistentRange == sequence.States.Length)
			{
				depth = 0;
				return;
			}
			depth = sequence.MaxGeneration;
			sequence = sequence.Update(NeighborSequences(), ref depth, ref errors,sequence.States.Length, optimize);
			depth = sequence.MaxGeneration - depth;
		}

		public void Evolve(bool optimize)
		{
			depth = sequence.MaxGeneration+1;
			sequence = sequence.Evolve(NeighborSequences(),ref depth, ref errors, optimize);
			depth = sequence.MaxGeneration - depth;
		}

		public void Truncate(int maxDepth)
		{
			if (sequence.States.Length > maxDepth)
				sequence = sequence.Truncate(maxDepth);
		}

	}
}