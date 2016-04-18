using System;
using System.Collections.Generic;

namespace TechniteSimulation
{
	
	public class Commit
	{
		public Commit Parent { get; protected set; }
		public readonly int Generation, Revision;
		public readonly Sector.ID ID;
		public State state;

		//public Commit(int gen, int rev, Commit parent, Sector.ID sid)
		//{
		//	Generation = gen;
		//	Revision = rev;
		//	Parent = parent;
		//	ID = sid;
		//	state = new State(sid);
		//}
		public override string ToString()
		{
			return Generation.ToString() + '/' + Revision;
		}

		public Commit FindGeneration(int gen)
		{
			Commit rs = this;
			while (rs != null && rs.Generation > gen)
				rs = rs.Parent;
			return rs;
		}

		public Commit[] otherParents;
		//public readonly int ID;
		//private static int currentID = 0;

		const bool StrictAdaption = true;

		bool	IsStable
		{
			get
			{
				if (otherParents != null)
					foreach(var p in this.otherParents)
					{
						if (p.Generation + 1 != Generation)
							return false;
						if (!p.IsStable)
							return false;
					}
				//if (Parent != null)
				//	return Parent.IsStable;
				return true;
			}
		}


		public static IEnumerable<Commit> AtGeneration(IEnumerable<Commit> otherParents, int generation)
		{
			foreach (var c in otherParents)
			{
				Commit p = c.FindGeneration(generation);
				if (p != null)
					yield return p;
			}
		}

		internal void CountCommits(HashSet<Commit> commits, ref int cnt)
		{
			if (commits.Contains(this))
				return;
			cnt++;
			commits.Add(this);
			if (Parent != null)
				Parent.CountCommits(commits, ref cnt);
			if (otherParents != null)
				foreach (Commit com in otherParents)
					com.CountCommits(commits, ref cnt);
		}

		public static IEnumerable<State> StatesOf(IEnumerable<Commit>  commits)
		{
			foreach (var c in commits)
				yield return c.state;
		}

		//internal Commit Update(IEnumerable<Commit> otherParents, ref int depth, ref int errors)
		//{
		//	List<Commit> newList = new List<Commit>();
		//	HashSet<Commit> newSet = new HashSet<Commit>();
		//	foreach (var c in AtGeneration(otherParents, Generation - 1))
		//	{
		//		newList.Add(c);
		//		newSet.Add(c);
		//	}
		//	if (this.otherParents != null)
		//	{
		//		HashSet<Commit> oldSet = new HashSet<Commit>();
		//		bool allGood = true;
		//		foreach (var p in this.otherParents)
		//		{
		//			if (!newSet.Contains(p))
		//				allGood = false;
		//			oldSet.Add(p);
		//		}
		//		if (allGood)
		//			foreach (var p in newList)
		//			{
		//				if (!oldSet.Contains(p))
		//				{
		//					allGood = false;
		//					break;
		//				}
		//			}
		//		if (allGood)
		//			return this;

		//		bool brk = true;
		//	}
		//	else
		//		if (newList.Count == 0 || Parent == null)
		//		{
		//			if (Parent == null && Generation > 0)
		//				errors ++;
		//			return this;
		//		}



		//	Commit parent = ((Commit)Parent).Update(newList, ref depth, ref errors);

		//	State newState = parent.state.Evolve(StatesOf(newList));
		//	bool adapt = newState == this.state;

		//	if (StrictAdaption && parent != Parent)
		//		adapt = false;

		//	depth = Math.Min(depth, Generation);
		//	Commit rs;
		//	if (adapt)
		//	{
		//		rs = this;
		//		rs.Parent = parent;
		//	}
		//	else
		//	{
		//		rs = new Commit(Generation, Revision + 1, parent,parent.ID);
		//		rs.state = newState;
		//	}

		//	rs.otherParents = newList.ToArray();
		//	return rs;
		//}

		internal void Truncate(int maxDepth)
		{
			if (maxDepth <= 0)
			{
				Parent = null;
				otherParents = null;
				return;
			}
			if (Parent != null)
				Parent.Truncate(maxDepth - 1);
		}

		//public static Random rnd = new Random();
		//internal Commit GetRandomParentTo(Commit knownHead)
		//{
		//	Commit rs = this;
		//	while (rs != knownHead)
		//	{
		//		if (rnd.NextDouble() < 0.5)
		//			return rs;
		//		if (knownHead == null || knownHead.Generation != rs.Generation)
		//			return knownHead;
		//		rs = rs.localParent;
		//	}
		//	return knownHead;
		//}

		//internal Commit Evolve(IEnumerable<Commit> n, ref int depth, ref int errors)
		//{
		//	Commit self = Update(n, ref depth, ref errors);
		//	Commit rs = new Commit(self.Generation+1,0,self,self.ID);
		//	return rs.Update(n, ref depth, ref errors);
		//}

	}
}