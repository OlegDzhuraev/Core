#if INSANE_TEAMS_EXTENSION
using System;
using UnityEngine;

namespace InsaneOne.Core.Teams
{
	[Serializable]
	public class TeamEnemyRule
	{
		[Min(0)] public int teamA;
		[Min(0)] public int teamB;

		public bool IsSame(TeamEnemyRule otherRule) => IsSame(otherRule.teamA, otherRule.teamB);

		public bool IsSame(int inTeamA, int inTeamB)
		{
			if (teamA == inTeamA && teamB == inTeamB)
				return true;

			if (teamB == inTeamA && teamA == inTeamB)
				return true;

			return false;
		}
	}
}
#endif