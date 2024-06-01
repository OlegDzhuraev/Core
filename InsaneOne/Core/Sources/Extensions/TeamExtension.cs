using UnityEngine;

namespace Game
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
	}
}