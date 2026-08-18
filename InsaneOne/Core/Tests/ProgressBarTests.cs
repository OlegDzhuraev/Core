using InsaneOne.Core.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class ProgressBarTests
	{
		GameObject root;
		ProgressBar progressBar;
		Image fillBar;
		TMP_Text titleText;
		TMP_Text numberText;

		[SetUp]
		public void SetUp()
		{
			root = new GameObject(nameof(ProgressBar));
			progressBar = root.AddComponent<ProgressBar>();

			fillBar = new GameObject("FillBar", typeof(Image)).GetComponent<Image>();
			titleText = new GameObject("Title", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
			numberText = new GameObject("Number", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();

			fillBar.transform.SetParent(root.transform);
			titleText.transform.SetParent(root.transform);
			numberText.transform.SetParent(root.transform);

			progressBar.SetupInternal(fillBar, titleText, numberText);
		}

		[TearDown]
		public void TearDown() => Object.DestroyImmediate(root);

		[Test]
		public void TestSetViewModelAppliesFillAmountImmediately()
		{
			// regression: Changed used to fire during the view model's own constructor, before ProgressBar
			// could subscribe to it, so a freshly-created view model never visually applied until the next Change() call
			progressBar.SetViewModel(new ProgressBarViewModel(3, 10));

			Assert.AreEqual(0.3f, fillBar.fillAmount, 0.0001f);
		}

		[Test]
		public void TestSetViewModelAppliesTitleImmediately()
		{
			// regression: titleText was never actually assigned anywhere, despite Title being part of the view model
			progressBar.SetViewModel(new ProgressBarViewModel(3, 10, "Kill enemies"));

			Assert.AreEqual("Kill enemies", titleText.text);
		}

		[Test]
		public void TestSetViewModelAppliesNumberTextImmediately()
		{
			progressBar.SetViewModel(new ProgressBarViewModel(3, 10));

			Assert.AreEqual("3/10", numberText.text);
		}

		[Test]
		public void TestChangeUpdatesFillAmountAfterInitialSet()
		{
			var viewModel = new ProgressBarViewModel(3, 10);
			progressBar.SetViewModel(viewModel);

			viewModel.Change(7, 10);

			Assert.AreEqual(0.7f, fillBar.fillAmount, 0.0001f);
			Assert.AreEqual("7/10", numberText.text);
		}

		[Test]
		public void TestSwitchingViewModelUnsubscribesFromThePrevious()
		{
			var firstViewModel = new ProgressBarViewModel(1, 10);
			progressBar.SetViewModel(firstViewModel);
			progressBar.SetViewModel(new ProgressBarViewModel(5, 10));

			firstViewModel.Change(9, 10); // should no longer affect progressBar, it moved on to the second view model

			Assert.AreEqual(0.5f, fillBar.fillAmount, 0.0001f);
		}
	}
}
