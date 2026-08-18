using System;
using UnityEngine;

namespace InsaneOne.Core.Goals
{
	/// <summary> Describes a single configurable goal (achievement-like objective) — its id, display title and how much progress completes it. </summary>
	[Serializable]
	public class GoalDefinition
	{
		public string Id => id;
		public string Title => title;
		public int RequiredAmount => requiredAmount;

		[SerializeField] string id;
		[SerializeField] string title;
		[Min(1)]
		[SerializeField] int requiredAmount = 1;

		public GoalDefinition() { }

		public GoalDefinition(string id, string title, int requiredAmount)
		{
			this.id = id;
			this.title = title;
			this.requiredAmount = requiredAmount;
		}
	}
}
