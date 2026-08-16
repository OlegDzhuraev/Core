#if INSANE_TEAMS_EXTENSION
using System.Collections.Generic;
using UnityEngine;

namespace InsaneOne.Core.Teams
{
	public static class TeamExtension
	{
		public static int GetTeam(this GameObject go)
		{
			if (!go.TryGetComponent<ITeamed>(out var teamBehaviour))
				return -1;

			return teamBehaviour.GetTeam();
		}

		public static void SetTeam(this GameObject go, int newTeam)
		{
			if (!go.TryGetComponent<ITeamed>(out var teamBehaviour))
				teamBehaviour = go.AddComponent<TeamBehaviour>();

			teamBehaviour.ChangeTeam(newTeam);
		}

		public static bool IsInSameTeam(this GameObject go, GameObject other)
		{
			var team = go.GetTeam();
			return team >= 0 && team == other.GetTeam();
		}

		/// <summary> Returns true if this object in team, which is enemy to other object. To use this method, you need to create asset TeamsSettings.
		/// You can set up teams relations in this asset. </summary>
		public static bool IsTeamEnemyTo(this GameObject go, GameObject other)
		{
			return TeamsSettings.Get().IsEnemies(go.GetTeam(), other.GetTeam());
		}

		/// <summary> Returns the display name configured for this object's team in TeamsSettings, or a generic fallback if none is set up. </summary>
		public static string GetTeamName(this GameObject go) => TeamsSettings.Get().GetTeamName(go.GetTeam());

		/// <summary> Returns the color configured for this object's team in TeamsSettings, or white if none is set up. </summary>
		public static Color GetTeamColor(this GameObject go) => TeamsSettings.Get().GetTeamColor(go.GetTeam());

		/// <summary> Gets components of type T from all physical objects in radius, which are in a team enemy to origin. Provide list to output results. Note that it will be overridden with new values. </summary>
		public static void GetEnemiesInSphere<T>(this GameObject origin, float radius, List<T> output, int layerMask = Physics.AllLayers) where T : Component
		{
			PhysicsExtensions.GetObjectsOfTypeInSphere(origin.transform.position, radius, output, layerMask);
			output.RemoveAll(target => !origin.IsTeamEnemyTo(target.gameObject));
		}

		/// <summary> Gets components of type T from all physical objects in radius, which are in the same team as origin (origin itself excluded). Provide list to output results. Note that it will be overridden with new values. </summary>
		public static void GetAlliesInSphere<T>(this GameObject origin, float radius, List<T> output, int layerMask = Physics.AllLayers) where T : Component
		{
			PhysicsExtensions.GetObjectsOfTypeInSphere(origin.transform.position, radius, output, layerMask);
			output.RemoveAll(target => target.gameObject == origin || !origin.IsInSameTeam(target.gameObject));
		}
	}
}
#endif