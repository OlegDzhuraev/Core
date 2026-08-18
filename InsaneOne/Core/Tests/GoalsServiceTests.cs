using InsaneOne.Core.Goals;
using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class GoalsServiceTests
	{
		const string KillsGoalId = "kills";
		const string SaveTestFileName = "goals_progress.tests.json";

		GoalsConfig CreateConfig()
		{
			var config = ScriptableObject.CreateInstance<GoalsConfig>();
			config.AddGoalInternal(new GoalDefinition(KillsGoalId, "Kill enemies", 15));

			return config;
		}

		[TearDown]
		public void TearDown() => GoalsService.DeleteSave(SaveTestFileName);

		[Test]
		public void TestInitialProgressIsZero()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			Assert.IsTrue(service.TryGetProgress(KillsGoalId, out var progress));
			Assert.AreEqual(0, progress.Current);
			Assert.IsFalse(progress.IsCompleted);
		}

		[Test]
		public void TestAddProgress()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			var progress = service.AddProgress(KillsGoalId, 5);
			Assert.AreEqual(5, progress.Current);

			progress = service.AddProgress(KillsGoalId, 3);
			Assert.AreEqual(8, progress.Current);
		}

		[Test]
		public void TestProgressIsClampedToRequiredAmount()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			var progress = service.AddProgress(KillsGoalId, 999);
			Assert.AreEqual(15, progress.Current);
			Assert.IsTrue(progress.IsCompleted);
		}

		[Test]
		public void TestProgressIsClampedToZero()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			var progress = service.AddProgress(KillsGoalId, -5);
			Assert.AreEqual(0, progress.Current);
		}

		[Test]
		public void TestUnknownGoalIdReturnsDefault()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			var progress = service.AddProgress("unknown-id", 5);
			Assert.IsNull(progress.Definition);

			Assert.IsFalse(service.TryGetProgress("unknown-id", out _));
		}

		[Test]
		public void TestProgressChangedEventFires()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			GoalProgress? received = null;
			service.ProgressChanged += p => received = p;

			service.AddProgress(KillsGoalId, 4);

			Assert.IsTrue(received.HasValue);
			Assert.AreEqual(4, received.Value.Current);
		}

		[Test]
		public void TestCompletedFiresOnlyOnce()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			var completedCount = 0;
			service.Completed += _ => completedCount++;

			service.AddProgress(KillsGoalId, 15);
			service.AddProgress(KillsGoalId, 15); // already completed, should stay at 15 and not fire again after clamp

			Assert.AreEqual(1, completedCount);
		}

		[Test]
		public void TestSetProgressOverridesValue()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			service.SetProgress(KillsGoalId, 10);
			var progress = service.SetProgress(KillsGoalId, 2);

			Assert.AreEqual(2, progress.Current);
		}

		[Test]
		public void TestResetProgress()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			service.AddProgress(KillsGoalId, 10);
			service.ResetProgress();

			Assert.IsTrue(service.TryGetProgress(KillsGoalId, out var progress));
			Assert.AreEqual(0, progress.Current);
		}

		[Test]
		public void TestHasSaveIsFalseBeforeFirstSave()
		{
			Assert.IsFalse(GoalsService.HasSave(SaveTestFileName));
		}

		[Test]
		public void TestSaveWritesJsonFile()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();
			service.AddProgress(KillsGoalId, 7);

			service.Save(SaveTestFileName);

			Assert.IsTrue(GoalsService.HasSave(SaveTestFileName));

			var json = System.IO.File.ReadAllText(GoalsService.GetSavePath(SaveTestFileName));
			StringAssert.Contains(KillsGoalId, json);
			StringAssert.Contains("7", json);
		}

		[Test]
		public void TestLoadRestoresSavedProgress()
		{
			var savingService = new GoalsService(CreateConfig());
			savingService.Init();
			savingService.AddProgress(KillsGoalId, 9);
			savingService.Save(SaveTestFileName);

			var loadingService = new GoalsService(CreateConfig());
			loadingService.Init();

			var loaded = loadingService.Load(SaveTestFileName);

			Assert.IsTrue(loaded);
			Assert.IsTrue(loadingService.TryGetProgress(KillsGoalId, out var progress));
			Assert.AreEqual(9, progress.Current);
		}

		[Test]
		public void TestLoadReturnsFalseWhenNoSaveFileExists()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();

			Assert.IsFalse(service.Load(SaveTestFileName));
		}

		[Test]
		public void TestLoadDoesNotFireEvents()
		{
			var savingService = new GoalsService(CreateConfig());
			savingService.Init();
			savingService.AddProgress(KillsGoalId, 5);
			savingService.Save(SaveTestFileName);

			var loadingService = new GoalsService(CreateConfig());
			loadingService.Init();

			var fired = false;
			loadingService.ProgressChanged += _ => fired = true;
			loadingService.Completed += _ => fired = true;

			loadingService.Load(SaveTestFileName);

			Assert.IsFalse(fired);
		}

		[Test]
		public void TestLoadIgnoresUnknownGoalIds()
		{
			var savingConfig = ScriptableObject.CreateInstance<GoalsConfig>();
			savingConfig.AddGoalInternal(new GoalDefinition("removed-goal", "Removed goal", 5));

			var savingService = new GoalsService(savingConfig);
			savingService.Init();
			savingService.AddProgress("removed-goal", 3);
			savingService.Save(SaveTestFileName);

			var loadingService = new GoalsService(CreateConfig()); // config without "removed-goal"
			loadingService.Init();

			Assert.DoesNotThrow(() => loadingService.Load(SaveTestFileName));
			Assert.IsFalse(loadingService.TryGetProgress("removed-goal", out _));
		}

		[Test]
		public void TestDeleteSaveRemovesFile()
		{
			var service = new GoalsService(CreateConfig());
			service.Init();
			service.Save(SaveTestFileName);

			Assert.IsTrue(GoalsService.HasSave(SaveTestFileName));

			GoalsService.DeleteSave(SaveTestFileName);

			Assert.IsFalse(GoalsService.HasSave(SaveTestFileName));
		}
	}
}
