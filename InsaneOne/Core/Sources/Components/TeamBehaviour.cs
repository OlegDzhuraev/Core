#if INSANE_TEAMS_EXTENSION
using System;
using InsaneOne.Core.Architect;
using UnityEngine;

namespace InsaneOne.Core
{
	/// <summary> Useful if your game have different teams for players/NPCs. GameObject class have extension methods GetTeam and SetTeam to simplify work with this component. </summary>
	public class TeamBehaviour : MonoBehaviour, ITeamed
	{
		public event Action<int> ChangedTeam;

		[SerializeField, Min(0)] int startTeam;

		int team = -1;
		bool initialTeamAnnounced;

		void Awake() => team = startTeam;

		/// <summary> Announces the starting team once all Awake calls in the scene are done, so listeners that
		/// subscribe to ChangedTeam in their own Awake don't miss it (Unity guarantees every Awake runs before any
		/// Start). Skipped if ChangeTeam was already called - and so already announced - between Awake and Start. </summary>
		void Start()
		{
			if (!initialTeamAnnounced)
				ChangedTeam?.Invoke(team);
		}

		public void ChangeTeam(int newTeam)
		{
			initialTeamAnnounced = true;

			if (newTeam == team)
				return;

			team = newTeam;
			ChangedTeam?.Invoke(team);
		}

		public int GetTeam() => team;
	}
}
#endif