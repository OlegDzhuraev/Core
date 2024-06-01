using System;
using UnityEngine;

namespace Game
{
	/// <summary> Useful if your game have different teams for players/NPCs. GameObject class have extension methods GetTeam and SetTeam to simplify work with this component. </summary>
	public class TeamBehaviour : MonoBehaviour
	{
		public event Action<int> ChangedTeam;

		[SerializeField] int startTeam;

		public int Team { get; private set; }

		void Start() => ChangeTeam(startTeam);

		public void ChangeTeam(int newTeam)
		{
			Team = newTeam;
			ChangedTeam?.Invoke(Team);
		}
	}
}