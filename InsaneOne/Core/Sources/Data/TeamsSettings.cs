using System;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> You can set up teams relations in this asset.  </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Teams Settings")]
	public class TeamsSettings : ScriptableObject
	{
		[Tooltip("Setup ids of teams, which should recognize other team as enemy. Example: If you set teamA to 0, and teamB to 1, teams 0 and 1 will be enemies.")]
		[SerializeField] TeamEnemyRule[] enemiesTeamsRules = Array.Empty<TeamEnemyRule>();

		public bool IsEnemies(int teamA, int teamB)
		{
			if (teamA == teamB || teamA < 0 || teamB < 0)
				return false;

			foreach (var teamEnemyRule in enemiesTeamsRules)
				if (teamEnemyRule.IsSame(teamA, teamB))
					return true;

			return false;
		}

		/// <summary> Recommended to use it only for test purposes. </summary>
		public void AddEnemyRuleInternal(TeamEnemyRule newTeamEnemyRule)
		{
			Array.Resize(ref enemiesTeamsRules, enemiesTeamsRules.Length + 1);
			enemiesTeamsRules[^1] = newTeamEnemyRule;
		}

		/// <summary> Recommended to use it only for test purposes. </summary>
		public TeamEnemyRule[] GetAllRulesInternal() => enemiesTeamsRules;

		/// <summary> Gets TeamsSettings asset from the CoreData asset. </summary>
		public static TeamsSettings Get()
		{
			var coreData = CoreData.Load();

			if (coreData && coreData.TeamsSettings)
				return coreData.TeamsSettings;

			Debug.LogError("[InsaneOne.Core] No CoreData found! TeamsSettings not work correctly!");
			return CreateInstance<TeamsSettings>();
		}
	}

	[Serializable]
	public class TeamEnemyRule
	{
		[Min(0)] public int teamA;
		[Min(0)] public int teamB;

		public bool IsSame(TeamEnemyRule otherRule) => IsSame(otherRule.teamA, otherRule.teamB);

		public bool IsSame(int inputTeamA, int inputTeamB)
		{
			if (teamA == inputTeamA && teamB == inputTeamB)
				return true;

			if (teamB == inputTeamA && teamA == inputTeamB)
				return true;

			return false;
		}
	}
}