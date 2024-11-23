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

		void Awake() => ChangeTeam(startTeam);

		public void ChangeTeam(int newTeam)
		{
			if (newTeam == team)
				return;

			team = newTeam;
			ChangedTeam?.Invoke(team);
		}

		public int GetTeam() => team;
	}
}