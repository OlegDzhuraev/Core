using System;

namespace InsaneOne.Core.Architect
{
	public interface ITeamed
	{
		public event Action<int> ChangedTeam;

		public int GetTeam();
		public void ChangeTeam(int newTeam);
	}
}