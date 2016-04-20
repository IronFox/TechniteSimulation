using System;
using System.Collections.Generic;
using System.Linq;

namespace TechniteSimulation
{
	public class Sequence
	{
		public readonly State[]		States;
		public readonly int			GenerationOffset,
									ConsistentRange;
		public readonly Sector.ID	MyID,
									PrimaryInconsistencySource;

		public int MaxGeneration { get { return GenerationOffset + States.Length - 1; } }

		public Sequence(IEnumerable<State> states, int generation, int numConsistent, Sector.ID myID, Sector.ID primaryInconsistencySource)
		{
			States = states.ToArray();
			GenerationOffset = generation;
			ConsistentRange = numConsistent;
			MyID = myID;
			PrimaryInconsistencySource = primaryInconsistencySource;
		}

		internal Sequence Update(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, ref int depth, ref int errors, int numGenerations, bool optimize)
		{
			List<State> newStates = new List<State>();
			int consistentTo = 0;
			bool consistent = true;
			List<State> newList = new List<State>();
			Sector.ID primaryInconsistencySource = MyID;
			for (int stateI = 0; stateI < numGenerations; stateI++)
			{
				int g = stateI + GenerationOffset;
				if (stateI == 0 || (optimize && stateI < ConsistentRange))
				{
					newStates.Add(States[stateI]);
					consistentTo++;
				}
				else
				{
					State b = newStates[newStates.Count() - 1];
					foreach (var c in AtGeneration(otherParents, g - 1))
					{
						if (c.Item2.HasValue)
						{
							if (MyID.RelevantToEvolution(c.Item1,g))
							{
								newList.Add(c.Item2.Value);

								if (c.Item3 == StateType.Inconsistent && consistent)
								{
									primaryInconsistencySource = c.Item1;
									consistent = false;
								}
							}
						}
						else
						{
							if (MyID.RelevantToEvolution(c.Item1, g))
							{
								if (c.Item3 == StateType.Truncated)
									errors++;
								if (consistent)
								{
									primaryInconsistencySource = c.Item1;
									consistent = false;
								}
							}
						}
						
					}

					var reference = stateI < States.Length ? new State?(States[stateI]) : null;
					newStates.Add(b.Evolve(newList, reference));
					newList.Clear();
					if (g < depth && reference.HasValue && newStates[newStates.Count() - 1] != reference.Value)
					{
						if (stateI == 1)
							errors++;
						depth = g;
					}
					if (consistent)
						consistentTo++;
				}
			}
			return new Sequence(newStates, GenerationOffset, consistentTo,MyID,primaryInconsistencySource);
		}

		public enum StateType
		{
			Okay,
			Inconsistent,
			Unavailable,
			Truncated
		}
		private static IEnumerable<Tuple<Sector.ID, State?, StateType>> AtGeneration(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, int generation)
		{
			foreach (var c in otherParents)
			{
				if (c.Item2 == null)
				{
					yield return new Tuple<Sector.ID, State?, StateType>(c.Item1, null, StateType.Unavailable);
				}
				else
				{
					StateType consistent;
					State? p = c.Item2.FindGeneration(generation, out consistent);
					yield return new Tuple<Sector.ID, State?, StateType>(c.Item1, p, consistent);
				}
			}
		}

		private State? FindGeneration(int generation, out StateType consistent)
		{
			consistent = StateType.Inconsistent;
			if (generation < GenerationOffset)
			{
				consistent = StateType.Truncated;
				return null;
			}
			int at = generation - GenerationOffset;
			if (at >= States.Length)
			{
				consistent = StateType.Inconsistent;
				return States[States.Length - 1];
			}
			consistent = at < ConsistentRange ? StateType.Okay : StateType.Inconsistent;
			return States[at];
		}

		internal Sequence Evolve(IEnumerable<Tuple<Sector.ID, Sequence>> n, ref int depth, ref int errors, bool optimize)
		{
			return Update(n, ref depth, ref errors,States.Length+1, optimize);
		}

		internal Sequence Truncate(int maxDepth)
		{
			int skip = States.Length - maxDepth;
			return new Sequence(States.Skip(skip), GenerationOffset + skip, Math.Max(0, ConsistentRange - skip),MyID,PrimaryInconsistencySource);
		}
	}
}