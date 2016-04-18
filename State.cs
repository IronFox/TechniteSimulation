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
				if (rng.NextDouble() < 0.4 / Math.Sqrt(SectorID.QuadraticDistanceTo(st.SectorID)))
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



		public static bool operator ==(State a, State b)
		{
			return a.value == b.value;
		}

		public static bool operator !=(State a, State b) { return !(a == b); }
	}

}
