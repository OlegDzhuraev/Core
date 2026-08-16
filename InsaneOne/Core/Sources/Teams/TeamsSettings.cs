#if INSANE_TEAMS_EXTENSION
using System;
using InsaneOne.Core.Utility;
using UnityEngine;

namespace InsaneOne.Core.Teams
{
	/// <summary> You can set up teams relations in this asset.  </summary>
	[CreateAssetMenu(menuName = "InsaneOne/Teams Settings")]
	public class TeamsSettings : ScriptableObject
	{
		static TeamsSettings fallbackInstance;

		[Tooltip("Display name and color per team. A team's id is its index in this list - team 0 is teamInfos[0], team 1 is teamInfos[1], etc. Every team id you use must have a matching entry here.")]
		[SerializeField]
		TeamInfo[] teamInfos =
		{
			new (Color.red),
			new (Color.blue),
		};

		[Tooltip("Setup ids of teams, which should recognize other team as enemy. Example: If you set teamA to 0, and teamB to 1, teams 0 and 1 will be enemies.")]
		[SerializeField] TeamEnemyRule[] enemiesTeamsRules = Array.Empty<TeamEnemyRule>();

		/// <summary> Number of teams configured in TeamInfos - valid team ids are 0..GetTeamCount()-1. </summary>
		public int GetTeamCount() => teamInfos.Length;

		public bool IsEnemies(int teamA, int teamB)
		{
			if (teamA == teamB || teamA < 0 || teamB < 0)
				return false;

			foreach (var teamEnemyRule in enemiesTeamsRules)
				if (teamEnemyRule.IsSame(teamA, teamB))
					return true;

			return false;
		}

		/// <summary> Returns the display name configured for the given team (its index in TeamInfos), or a generic fallback ("No Team" / "Team N") if it has no matching entry or the entry's name is empty. </summary>
		public string GetTeamName(int team)
		{
			if (TryGetTeamInfo(team, out var info) && !string.IsNullOrEmpty(info.name))
				return $"[{team}] {info.name}";

			return team < 0 ? "No Team" : $"[{team}] Unnamed Team";
		}

		/// <summary> Returns the color configured for the given team (its index in TeamInfos), or white if it has no matching entry. </summary>
		public Color GetTeamColor(int team) => TryGetTeamInfo(team, out var info) ? info.color : Color.white;

		bool TryGetTeamInfo(int team, out TeamInfo info)
		{
			if (team >= 0 && team < teamInfos.Length)
			{
				info = teamInfos[team];
				return true;
			}

			info = null;

			// -1 is the "no team" sentinel, not a missing config - only warn about actual team ids that should have an entry.
			if (team >= 0)
				CoreUnityLogger.I.Log($"Team {team} has no matching entry in TeamInfos (list has {teamInfos.Length} entries) - add one at index {team} so its name/color are defined.", LogLevel.Warning);

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

		/// <summary> Recommended to use it only for test purposes. </summary>
		public TeamInfo[] GetAllTeamInfosInternal() => teamInfos;

		/// <summary> Gets TeamsSettings asset from the CoreData asset. </summary>
		public static TeamsSettings Get()
		{
			if (CoreData.TryLoad(out var coreData) && coreData.TeamsSettings)
				return coreData.TeamsSettings;

			CoreUnityLogger.I.Log($"No {nameof(CoreData)} found! {nameof(TeamsSettings)} not work correctly!", LogLevel.Error);

			if (!fallbackInstance)
				fallbackInstance = CreateInstance<TeamsSettings>();

			return fallbackInstance;
		}
	}
}
#endif