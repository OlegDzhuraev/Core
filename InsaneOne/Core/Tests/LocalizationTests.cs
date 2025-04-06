using System;
using InsaneOne.Core.Locales;
using NUnit.Framework;

namespace InsaneOne.Core.Tests
{
	/// <summary> These tests require prepared Localization.csv file.</summary>
	[TestFixture]
	public class LocalizationTests
	{
		[Test]
		public void TestInitialize()
		{
			Localization.Initialize();
			Assert.True(Localization.IsLoaded);

			Localization.Unload();
		}

		[Test]
		public void TestNoFile()
		{
			Assert.Throws<Exception>(() => Localization.Initialize("Custom.csv"));

			Localization.Unload();
		}

		[Test]
		public void TestNotInitializedSetLang()
		{
			Assert.Throws<Exception>(() => Localization.SetLanguage("English"));
		}

		[Test]
		public void TestLanguage()
		{
			Localization.Initialize();
			Localization.SetLanguage("French");

			var result = Localization.GetText("LanguageTest");
			Assert.AreEqual(result, "Fr_1");

			Localization.Unload();
		}

		[Test]
		public void TestFallback()
		{
			Localization.Initialize();
			Localization.SetLanguage("French");

			var result = Localization.GetText("FallbackTest");
			Assert.AreEqual(result, "En_2");

			Localization.Unload();
		}
	}
}