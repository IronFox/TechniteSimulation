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

		internal Sequence Update(IEnumerable<Sequence> otherParents, ref int depth, ref int errors, int numGenerations)
		{
			List<State> newStates = new List<State>();
			int consistentTo = 0;
			bool consistent = true;
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
					List<State> newList = new List<State>();
					HashSet<State> newSet = new HashSet<State>();
					foreach (var c in AtGeneration(otherParents, g - 1))
					{
						if (c.Item1.HasValue)
						{
							newList.Add(c.Item1.Value);
							newSet.Add(c.Item1.Value);
						}
						else
						{
							errors++;
							consistent = false;
						}
						if (!c.Item2)
							consistent = false;
					}

					State b = newStates[newStates.Count()-1];
					var reference = stateI < States.Length ? new State?(States[stateI]) : null;
					newStates.Add(b.Evolve(newList, reference));
					if (g < depth && reference.HasValue && newStates[newStates.Count() - 1] != reference.Value)
						depth = g;
					if (consistent)
						consistentTo++;
				}
			}
			return new Sequence(newStates, GenerationOffset, consistentTo);
		}

		private static IEnumerable<Tuple<State?, bool>> AtGeneration(IEnumerable<Sequence> otherParents, int generation)
		{
			foreach (var c in otherParents)
			{
				if (c == null)
				{
					yield return new Tuple<State?, bool>(null, false);
				}
				else
				{
					bool consistent = false;
					State? p = c.FindGeneration(generation, out consistent);
					yield return new Tuple<State?, bool>(p, consistent);
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

		internal Sequence Evolve(IEnumerable<Sequence> n, ref int depth, ref int errors)
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