using System.Collections;
using InsaneOne.Core.Architect;
using InsaneOne.Core.Goals;
using InsaneOne.Core.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace InsaneOne.Core.Tests
{
	/// <summary> Runtime (Play Mode) tests for the goals system: GoalsService registered via ServiceLocator
	/// driving a GoalNotificationPopup through Show/Hide/queueing over actual frames. The ProgressBar it drives is left
	/// with no fillBar/titleText/numberText refs - popup and ProgressBar both null-check them, so these tests exercise
	/// the system without needing your prefab (title/fill rendering itself is covered by ProgressBarTests instead). </summary>
	[TestFixture]
	public class GoalsPlayModeTests
	{
		const string KillsGoalId = "kills";
		const string LootGoalId = "loot";

		GoalsConfig config;
		GoalsService service;
		GameObject popupGo;
		GoalNotificationPopup popup;

		[SetUp]
		public void SetUp()
		{
			ServiceLocator.Reset();

			config = ScriptableObject.CreateInstance<GoalsConfig>();
			config.AddGoalInternal(new GoalDefinition(KillsGoalId, "Kill enemies", 15));

			service = new GoalsService(config);
			ServiceLocator.Register(service); // Init() is called automatically here, as GoalsService implements IInitService

			popupGo = new GameObject(nameof(GoalNotificationPopup));
			var progressBar = popupGo.AddComponent<ProgressBar>(); // left with no fillBar/text refs, ProgressBar null-checks them

			popup = popupGo.AddComponent<GoalNotificationPopup>();
			popup.SetupInternal(progressBar, 0.2f);
		}

		[TearDown]
		public void TearDown()
		{
			if (popupGo)
				Object.Destroy(popupGo);

			Object.Destroy(config);
			ServiceLocator.Reset();
		}

		[UnityTest]
		public IEnumerator TestPopupIsHiddenInitially()
		{
			yield return null;

			Assert.IsFalse(popupGo.activeSelf);
		}

		[UnityTest]
		public IEnumerator TestUnboundPopupIgnoresProgress()
		{
			// popup.Bind was never called - a service change should not affect it
			ServiceLocator.Get<GoalsService>().AddProgress(KillsGoalId, 5);
			yield return null;

			Assert.IsFalse(popupGo.activeSelf);
		}

		[UnityTest]
		public IEnumerator TestPopupShowsWhenBoundServiceProgresses()
		{
			popup.Bind(ServiceLocator.Get<GoalsService>());

			service.AddProgress(KillsGoalId, 5);
			yield return null;

			Assert.IsTrue(popupGo.activeSelf);
			Assert.AreEqual(KillsGoalId, popup.ViewModel.Id);
			Assert.AreEqual("Kill enemies", popup.ViewModel.Title);
			Assert.AreEqual(5, popup.ViewModel.Current);
			Assert.AreEqual(15, popup.ViewModel.Required);
			Assert.IsFalse(popup.ViewModel.IsCompleted);
		}

		[UnityTest]
		public IEnumerator TestPopupHidesAfterShowDuration()
		{
			popup.Bind(service);

			service.AddProgress(KillsGoalId, 3);
			yield return null;
			Assert.IsTrue(popupGo.activeSelf);

			yield return new WaitForSeconds(0.35f); // longer than the 0.2s show duration set in SetUp

			Assert.IsFalse(popupGo.activeSelf);
		}

		[UnityTest]
		public IEnumerator TestUnbindStopsReactingToFurtherProgress()
		{
			popup.Bind(service);
			popup.Unbind();

			service.AddProgress(KillsGoalId, 5);
			yield return null;

			Assert.IsFalse(popupGo.activeSelf);
		}

		[UnityTest]
		public IEnumerator TestCompletedGoalIsReflectedInPopup()
		{
			popup.Bind(service);

			service.AddProgress(KillsGoalId, 999); // clamped to RequiredAmount
			yield return null;

			Assert.IsTrue(popupGo.activeSelf);
			Assert.AreEqual(15, popup.ViewModel.Current);
			Assert.IsTrue(popup.ViewModel.IsCompleted);
		}

		[UnityTest]
		public IEnumerator TestProgressOnTwoGoalsIsQueuedAndShownInOrder()
		{
			config.AddGoalInternal(new GoalDefinition(LootGoalId, "Collect loot", 10));
			popup.Bind(service);

			service.AddProgress(KillsGoalId, 1); // shown immediately
			service.AddProgress(LootGoalId, 2); // queued, popup is already busy
			yield return null;

			Assert.AreEqual(KillsGoalId, popup.ViewModel.Id);

			yield return new WaitForSeconds(0.35f); // first notification's duration elapses, second one takes over

			Assert.IsTrue(popupGo.activeSelf);
			Assert.AreEqual(LootGoalId, popup.ViewModel.Id);
			Assert.AreEqual(2, popup.ViewModel.Current);
		}
	}
}
