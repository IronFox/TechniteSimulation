using System.Collections.Generic;
using System.Linq;

namespace TechniteSimulation
{
	public class Sequence
	{
		public readonly State[]		States;
		public readonly int			GenerationOffset;

		public Sequence(IEnumerable<State> states, int generation)
		{
			this.States = states.ToArray();
			this.GenerationOffset = generation;
		}
	}
}