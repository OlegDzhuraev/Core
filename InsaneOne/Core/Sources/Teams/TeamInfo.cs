#if INSANE_TEAMS_EXTENSION
using System;
using UnityEngine;

namespace InsaneOne.Core.Teams
{
	[Serializable]
	public class TeamInfo
	{
		public string name = "Team";
		public Color color = Color.white;

		public TeamInfo() { }

		public TeamInfo(Color color)
		{
			this.color = color;
		}
	}
}
#endif