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
		int value;

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

		internal State Evolve(IEnumerable<State> enumerable, int revision)
		{
			State rs = new State(SectorID,revision);
			Random rng = new Random(value);
			rs.value = value;
			bool evolve = rng.NextDouble() < 0.6;
			foreach (var st in enumerable)
				if (rng.NextDouble() < 0.12 / Math.Sqrt(SectorID.QuadraticDistanceTo(st.SectorID)))
				{
					rs.value += st.value;
					evolve = true;
				}
			if (evolve)
				rs.value += rng.Next();

			return rs;
		}



		public static bool operator ==(State a, State b)
		{
			return a.value == b.value;
		}

		public static bool operator !=(State a, State b) { return !(a == b); }
	}

}
