using System;
using System.Collections.Generic;

namespace InsaneOne.Core.Goals
{
	/// <summary> JSON-serializable snapshot of a GoalsService's progress, written/read by its Save/Load methods. </summary>
	[Serializable]
	public class GoalsSaveData
	{
		public List<GoalSaveEntry> Entries = new ();
	}

	/// <summary> Progress of a single goal at the moment of saving. </summary>
	[Serializable]
	public class GoalSaveEntry
	{
		public string Id;
		public int Amount;
	}
}
