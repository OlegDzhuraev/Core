#if INSANE_TEAMS_EXTENSION
using System;

namespace InsaneOne.Core.Teams
{
	public interface ITeamed
	{
		public event Action<int> ChangedTeam;

		public int GetTeam();
		public void ChangeTeam(int newTeam);
	}
}
#endif