using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechniteSimulation
{
	public struct State
	{
		public readonly Sector.ID SectorID;
		public readonly int Revision;
		public int value;

		public State(Sector.ID sid, int rev, int value) 
		{
			this.SectorID = sid;
			this.Revision = rev;
			this.value = value;
		}
		public State(Sector.ID sid, int rev) : this()
		{
			this.SectorID = sid;
			this.Revision = rev;
			value = sid.GetHashCode();
		}

		public override int GetHashCode()
		{
			return SectorID.GetHashCode() * 17 + value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is State && ((State)obj) == this;
		}

		internal State Evolve(IEnumerable<State> enumerable, State? compareTo)
		{
			Random rng = new Random(value);
			int outValue = value;
			bool evolve = rng.NextDouble() < 0.6;
			foreach (var st in enumerable)
			{
				outValue += st.value;
				evolve = true;
			}
			if (evolve)
				outValue += rng.Next();

			return compareTo.HasValue
					? new State(SectorID, outValue != compareTo.Value.value ? compareTo.Value.Revision + 1 : compareTo.Value.Revision, outValue)
					: new State(SectorID, 0, outValue);
		}

		

		int xorshf96(int seed)
		{          //period 2^96-1
			int x = 123456789 * seed, y = 362436069 + seed, z = 521288629 ^seed;
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

		internal bool RelevantToEvolution(Sector.ID other, int generation)
		{

			//			val = val * 17 + generation;
			////			Random rng = new Random(val);
			//			float rnd = (float)(xorshf96(val)%0xFF) / 255.0f;
			//return true;
//			int val = other.GetHashCode() * 17 + SectorID.GetHashCode();
			double rnd = PerlinNoise.Global.Noise(SectorID.X * 100 + (other.X-SectorID.X) * 10, SectorID.Y * 100 + (other.Y-SectorID.Y) * 10, (double)generation / 20.0f/* + val*/);
			return Math.Abs(rnd) < 0.025 / (SectorID.QuadraticDistanceTo(other));
		}

		public static bool operator ==(State a, State b)
		{
			return a.value == b.value;
		}

		public static bool operator !=(State a, State b) { return !(a == b); }
	}

}
