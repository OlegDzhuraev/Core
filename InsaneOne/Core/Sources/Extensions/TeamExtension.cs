using UnityEngine;

namespace InsaneOne.Core
{
	public static class TeamExtension
	{
		public static int GetTeam(this GameObject go)
		{
			if (!go.TryGetComponent<TeamBehaviour>(out var teamBehaviour))
				return -1;

			return teamBehaviour.Team;
		}

		public static void SetTeam(this GameObject go, int newTeam)
		{
			if (!go.TryGetComponent<TeamBehaviour>(out var teamBehaviour))
				teamBehaviour = go.AddComponent<TeamBehaviour>();

			teamBehaviour.ChangeTeam(newTeam);
		}

		public static bool IsInSameTeam(this GameObject go, GameObject other) => go.GetTeam() == other.GetTeam();

		/// <summary> Returns true if this object in team, which is enemy to other object. To use this method, you need to create asset TeamsSettings.
		/// You can set up teams relations in this asset. </summary>
		public static bool IsTeamEnemyTo(this GameObject go, GameObject other)
		{
			return TeamsSettings.Get().IsEnemies(go.GetTeam(), other.GetTeam());
		}
	}
}