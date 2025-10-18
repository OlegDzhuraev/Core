#if INSANE_TEAMS_EXTENSION

using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class TeamsExtensionsTest
	{
		void Prepare(out GameObject goTeamA, out GameObject goTeamB, out GameObject goNoTeam)
		{
			goTeamA = new GameObject("Test Object Team A");
			goTeamA.SetTeam(0);

			goTeamB = new GameObject("Test Object Team B");
			goTeamB.SetTeam(1);

			goNoTeam = new GameObject("Test Object No Team");
		}

		[Test]
		public void TestGetTeam()
		{
			Prepare(out var goTeamA, out _, out _);
			Assert.AreEqual(0, goTeamA.GetTeam());
		}

		[Test]
		public void TestGetNoTeam()
		{
			Prepare(out _, out _, out var goNoTeam);
			Assert.AreEqual(-1, goNoTeam.GetTeam());
		}

		[Test]
		public void TestSameTeam()
		{
			Prepare(out var goTeamA, out var goTeamB, out _);

			var isSameTeam = goTeamA.IsInSameTeam(goTeamB);
			Assert.False(isSameTeam);
		}

		[Test]
		public void TestIsEnemies()
		{
			Prepare(out var goTeamA, out var goTeamB, out _);
			var teamsSettingsCustom = ScriptableObject.CreateInstance<TeamsSettings>();
			teamsSettingsCustom.AddEnemyRuleInternal(new TeamEnemyRule() {teamA = 0, teamB = 1});

			var isEnemies = teamsSettingsCustom.IsEnemies(goTeamA.GetTeam(), goTeamB.GetTeam());
			Assert.True(isEnemies);
		}

		[Test]
		public void TestNoEnemies()
		{
			Prepare(out var goTeamA, out var goTeamB, out _);
			var teamsSettingsCustom = ScriptableObject.CreateInstance<TeamsSettings>();

			var isEnemies = teamsSettingsCustom.IsEnemies(goTeamA.GetTeam(), goTeamB.GetTeam());
			Assert.False(isEnemies);
		}
	}
}

#endif