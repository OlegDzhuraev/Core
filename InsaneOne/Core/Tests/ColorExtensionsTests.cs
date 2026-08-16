using System;
using NUnit.Framework;
using UnityEngine;

namespace InsaneOne.Core.Tests
{
	[TestFixture]
	public class ColorExtensionsTests
	{
		[Test]
		public void TestGetHexValue_Color32_WithoutAlpha()
		{
			var color = new Color32(255, 0, 255, 0);

			Assert.AreEqual("#FF00FF", color.GetHexValue());
		}

		[Test]
		public void TestGetHexValue_Color32_WithAlpha()
		{
			var color = new Color32(255, 0, 255, 0);

			Assert.AreEqual("#FF00FF00", color.GetHexValue(true));
		}

		[Test]
		public void TestGetHexValue_Color_WithoutAlpha()
		{
			Color color = new Color32(255, 0, 255, 0);

			Assert.AreEqual("#FF00FF", color.GetHexValue());
		}

		// regression test: isNeededAlpha used to be silently dropped for the Color overload
		[Test]
		public void TestGetHexValue_Color_WithAlpha()
		{
			Color color = new Color32(255, 0, 255, 0);

			Assert.AreEqual("#FF00FF00", color.GetHexValue(true));
		}

		[Test]
		public void TestFromHex_ParsesRgbWithHash()
		{
			var color = ColorExtensions.FromHex("#FF00FF");

			Assert.AreEqual(new Color32(255, 0, 255, 255), color);
		}

		[Test]
		public void TestFromHex_ParsesRgbWithoutHash()
		{
			var color = ColorExtensions.FromHex("00FF00");

			Assert.AreEqual(new Color32(0, 255, 0, 255), color);
		}

		[Test]
		public void TestFromHex_ParsesRgba()
		{
			var color = ColorExtensions.FromHex("#11223344");

			Assert.AreEqual(new Color32(0x11, 0x22, 0x33, 0x44), color);
		}

		[Test]
		public void TestFromHex_IsCaseInsensitive()
		{
			var upper = ColorExtensions.FromHex("AABBCC");
			var lower = ColorExtensions.FromHex("aabbcc");

			Assert.AreEqual(upper, lower);
		}

		[Test]
		public void TestFromHex_ThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => ColorExtensions.FromHex(null));
		}

		[Test]
		public void TestFromHex_ThrowsOnInvalidLength()
		{
			Assert.Throws<ArgumentException>(() => ColorExtensions.FromHex("FFF"));
		}

		[Test]
		public void TestFromHex_RoundTripsWithGetHexValue()
		{
			var original = new Color32(0x12, 0x34, 0x56, 0x78);
			var hex = original.GetHexValue(true);

			var parsed = ColorExtensions.FromHex(hex);

			Assert.AreEqual(original, parsed);
		}

		[Test]
		public void TestGetWithHue_ChangesHueKeepsAlpha()
		{
			var red = new Color(1, 0, 0, 0.5f);

			var result = red.GetWithHue(1f / 3f); // 1/3 = green hue
			Color.RGBToHSV(result, out var hue, out _, out _);

			Assert.AreEqual(1f / 3f, hue, 0.01f);
			Assert.AreEqual(0.5f, result.a, 0.01f);
		}

		[Test]
		public void TestGetWithSaturation_Desaturates()
		{
			var saturated = Color.HSVToRGB(0.5f, 1f, 1f);

			var result = saturated.GetWithSaturation(0f);
			Color.RGBToHSV(result, out _, out var saturation, out _);

			Assert.AreEqual(0f, saturation, 0.01f);
			Assert.AreEqual(result.r, result.g, 0.01f);
			Assert.AreEqual(result.g, result.b, 0.01f);
		}

		[Test]
		public void TestGetWithValue_ChangesBrightnessKeepsAlpha()
		{
			var bright = new Color(1, 0, 0, 0.5f);

			var result = bright.GetWithValue(0.5f);
			Color.RGBToHSV(result, out _, out _, out var value);

			Assert.AreEqual(0.5f, value, 0.01f);
			Assert.AreEqual(0.5f, result.a, 0.01f);
		}
	}
}
