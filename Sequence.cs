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

		public int MaxGeneration { get { return GenerationOffset + States.Length - 1; } }

		public Sequence(IEnumerable<State> states, int generation, int numConsistent)
		{
			States = states.ToArray();
			GenerationOffset = generation;
			ConsistentRange = numConsistent;
		}

		internal Sequence Update(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, ref int depth, ref int errors, int numGenerations)
		{
			List<State> newStates = new List<State>();
			int consistentTo = 0;
			bool consistent = true;
			List<State> newList = new List<State>();
			for (int stateI = 0; stateI < numGenerations; stateI++)
			{
				int g = stateI + GenerationOffset;
				if (stateI == 0)
				{
					newStates.Add(States[0]);
					consistentTo++;
				}
				else
				{
					State b = newStates[newStates.Count() - 1];
					foreach (var c in AtGeneration(otherParents, g - 1))
					{
						if (c.Item2.HasValue)
						{
							if (b.RelevantToEvolution(c.Item1,g))
							{
								newList.Add(c.Item2.Value);

								if (!c.Item3)
									consistent = false;
							}
						}
						else
						{
							if (b.RelevantToEvolution(c.Item1, g))
							{
								errors++;
								consistent = false;
							}
						}
						
					}

					var reference = stateI < States.Length ? new State?(States[stateI]) : null;
					newStates.Add(b.Evolve(newList, reference));
					newList.Clear();
					if (g < depth && reference.HasValue && newStates[newStates.Count() - 1] != reference.Value)
						depth = g;
					if (consistent)
						consistentTo++;
				}
			}
			return new Sequence(newStates, GenerationOffset, consistentTo);
		}

		private static IEnumerable<Tuple<Sector.ID, State?, bool>> AtGeneration(IEnumerable<Tuple<Sector.ID, Sequence>> otherParents, int generation)
		{
			foreach (var c in otherParents)
			{
				if (c.Item2 == null)
				{
					yield return new Tuple<Sector.ID, State?, bool>(c.Item1, null, false);
				}
				else
				{
					bool consistent = false;
					State? p = c.Item2.FindGeneration(generation, out consistent);
					yield return new Tuple<Sector.ID, State?, bool>(c.Item1, p, consistent);
				}
			}
		}

		private State? FindGeneration(int generation, out bool consistent)
		{
			consistent = false;
			if (generation < GenerationOffset)
				return null;
			int at = generation - GenerationOffset;
			if (at >= States.Length)
				return null;
			consistent = at < ConsistentRange;
			return States[at];
		}

		internal Sequence Evolve(IEnumerable<Tuple<Sector.ID, Sequence>> n, ref int depth, ref int errors)
		{
			return Update(n, ref depth, ref errors,States.Length+1);
		}

		internal Sequence Truncate(int maxDepth)
		{
			int skip = States.Length - maxDepth;
			return new Sequence(States.Skip(skip), GenerationOffset + skip, Math.Max(0, ConsistentRange - skip));
		}
	}
}