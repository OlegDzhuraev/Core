using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Goals
{
	/// <summary> Configure the list of available goals (achievement-like objectives, e.g. "kill 15 enemies of some type") in this asset,
	/// then read/update their runtime progress through <see cref="GoalsService"/>. </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Goals Config")]
	public class GoalsConfig : ScriptableObject
	{
		public IReadOnlyList<GoalDefinition> Goals => goals;

		[SerializeField] List<GoalDefinition> goals = new ();

		/// <summary> Finds a goal definition by its id. </summary>
		public bool TryGetGoal(string id, out GoalDefinition goal)
		{
			foreach (var candidate in goals)
			{
				if (candidate.Id != id)
					continue;

				goal = candidate;
				return true;
			}

			goal = default;
			return false;
		}

		/// <summary> Recommended to use it only for test purposes. </summary>
		public void AddGoalInternal(GoalDefinition goal) => goals.Add(goal);
	}
}
