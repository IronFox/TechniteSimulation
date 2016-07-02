using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechniteSimulation
{
	public struct State
	{
		//public readonly Sector.ID SectorID;
		//public readonly int Revision;
		//public int value;
		public readonly ushort InconsistentCells;
		internal const ushort MaxInconsistency = (ushort)0x0660;

		public State(ushort incCells) 
		{
			InconsistentCells = incCells;
		}
		public State(Sector.ID sid) : this()
		{
		}

		public override int GetHashCode()
		{
			return InconsistentCells.GetHashCode();
				//SectorID.GetHashCode() * 17 + value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is State && ((State)obj) == this;
		}

		static void Set(ref ushort v, int bit, bool to)
		{
			if (to)
				v |= (ushort)(1 << bit);
			else
				v &= (ushort)~(1 << bit);
		}

		static bool Get(ushort v, int bit)
		{
			return ((v >> bit) & 0x1) != 0;
		}

		static void Include(ref ushort inc, Sector.IDDelta d, ushort other)
		{
			switch (d.X)
			{
				case -1:
					switch (d.Y)
					{
						case -1:
							{
								//12 = 6
								Set(ref inc, 12, Get(other, 6));
							}
							break;
						case 0:
							{
								Set(ref inc, 13, Get(other, 5));
								Set(ref inc, 14, Get(other, 6));
							}
							break;
						case 1:
							{
								Set(ref inc, 15, Get(other, 5));
							}
							break;
					}
					break;
				case 0:
					switch (d.Y)
					{
						case -1:
							{
								Set(ref inc, 4, Get(other, 6));
								Set(ref inc, 8, Get(other, 10));
							}
							break;
						case 1:
							{
								Set(ref inc, 7, Get(other, 5));
								Set(ref inc, 11, Get(other, 9));
							}
							break;
					}
					break;
				case 1:
					switch (d.Y)
					{
						case -1:
							{
								Set(ref inc, 0, Get(other, 10));
							}
							break;
						case 0:
							{
								Set(ref inc, 1, Get(other, 9));
								Set(ref inc, 2, Get(other, 10));
							}
							break;
						case 1:
							{
								Set(ref inc, 3, Get(other, 9));
							}
							break;
					}
					break;


			}

		}

		internal State Evolve(IEnumerable<Tuple<Sector.IDDelta, State?>> enumerable)
		{
			//	Random rng = new Random(value);
			//int outValue = value;
			//bool evolve = rng.NextDouble() < 0.6;
			ushort newInc = InconsistentCells;
			foreach (var st in enumerable)
			{
				ushort inc = st.Item2.HasValue ? st.Item2.Value.InconsistentCells : (ushort)0xffff;
				Include(ref newInc, st.Item1, inc);
			}
			ushort smeared = (ushort)(newInc
				| (newInc << 4) | (newInc >> 4)
				| (newInc << 1) | (newInc >> 1)
				| (newInc << 3) | (newInc >> 3)
				| (newInc << 5) | (newInc >> 5)
				);
			smeared &= MaxInconsistency;
			return new State(smeared);
		}

		

		public static bool operator ==(State a, State b)
		{
			return a.InconsistentCells == b.InconsistentCells;
		}

		public static bool operator !=(State a, State b) { return !(a == b); }
	}

}
