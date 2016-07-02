using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TechniteSimulation
{
	public class Sequence
	{
		public readonly State[]		States;
		public readonly int			GenerationOffset,
									ConsistentRange;
		public readonly Sector.ID	MyID;

		public int MaxGeneration { get { return GenerationOffset + States.Length - 1; } }

		public Sequence(IEnumerable<State> states, int generation, int numConsistent, Sector.ID myID)
		{
			States = states.ToArray();
			GenerationOffset = generation;
			ConsistentRange = numConsistent;
			MyID = myID;
		}

		internal Sequence Update(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, ref int depth, ref int errors, int numGenerations, bool optimize)
		{
			List<State> newStates = new List<State>();
			int consistentTo = 0;
			//bool consistent = true;
			List<Tuple<Sector.IDDelta, State?>> influence = new List<Tuple<Sector.IDDelta, State?>>();
			for (int stateI = 0; stateI < numGenerations; stateI++)
			{
				int g = stateI + GenerationOffset;
				if (stateI == 0 || (optimize && stateI < ConsistentRange))
				{
					Debug.Assert(States[stateI].InconsistentCells == 0);
					newStates.Add(States[stateI]);
					consistentTo++;
				}
				else
				{
					State b = newStates[newStates.Count() - 1];
					if (stateI == ConsistentRange)
						b = new State();
					foreach (var c in AtGeneration(otherParents, g - 1))
					{
						if (MyID.RelevantToEvolution(c.Item1,g))
						{
							influence.Add(new Tuple<Sector.IDDelta, State?>(c.Item1 - MyID,  c.Item2));
						}
					}

					var reference = stateI < States.Length ? new State?(States[stateI]) : null;
					State st = b.Evolve(influence);
					newStates.Add(st);
					influence.Clear();
					if (g < depth && reference.HasValue && st != reference.Value)
					{
						//if (stateI == 1)
							//errors++;
						depth = g;
					}
					bool consistent = st.InconsistentCells == 0;
					if (consistent)
						consistentTo++;
					else
						if (st.InconsistentCells == State.MaxInconsistency)
						{
						stateI++;
							for (; stateI < numGenerations; stateI++)
								newStates.Add(new State(State.MaxInconsistency));
								break;
						}
				}
			}
			return new Sequence(newStates, GenerationOffset, consistentTo,MyID);
		}


		private static IEnumerable<Tuple<Sector.ID, State?>> AtGeneration(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, int generation)
		{
			foreach (var c in otherParents)
			{
				if (c.Item2 == null)
				{
					yield return new Tuple<Sector.ID, State?>(c.Item1, null);
				}
				else
				{
					State? p = c.Item2.FindGeneration(generation);
					yield return new Tuple<Sector.ID, State?>(c.Item1, p);
				}
			}
		}

		private State? FindGeneration(int generation)
		{
			if (generation <= GenerationOffset)
			{
				return new State?(new State());//unknown
			}
			int at = generation - GenerationOffset;
			if (at >= States.Length)
			{
				return null;
				//return States[States.Length - 1];
			}
			return States[at];
		}

		internal Sequence Evolve(IEnumerable<Tuple<Sector.ID, Sequence>> n, ref int depth, ref int errors, bool optimize)
		{
			return Update(n, ref depth, ref errors,States.Length+1, optimize);
		}

		internal Sequence Truncate(int maxDepth, ref int errors)
		{
			int skip = States.Length - maxDepth;
			if (ConsistentRange <= skip)
				errors+=skip;
			return new Sequence(States.Skip(skip), GenerationOffset + skip, Math.Max(0, ConsistentRange - skip),MyID);
		}
	}
}