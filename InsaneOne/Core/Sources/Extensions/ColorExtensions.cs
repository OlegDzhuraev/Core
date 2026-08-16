using System;
using System.Globalization;
using UnityEngine;

namespace InsaneOne.Core
{
	public static class ColorExtensions
	{
		const string Format = "X2";

		public static Color32 GetWithR(this Color32 c, byte value) => new(value, c.g, c.b, c.a);
		public static Color GetWithR(this Color c, float value) => new(value, c.g, c.b, c.a);

		public static Color32 GetWithG(this Color32 c, byte value) => new(c.r, value, c.b, c.a);
		public static Color GetWithG(this Color c, float value) => new(c.r, value, c.b, c.a);

		public static Color32 GetWithB(this Color32 c, byte value) => new(c.r, c.g, value, c.a);
		public static Color GetWithB(this Color c, float value) => new(c.r, c.g, value, c.a);

		public static Color32 GetWithAlpha(this Color32 c, byte value) => new(c.r, c.g, c.b, value);
		public static Color GetWithAlpha(this Color c, float value) => new(c.r, c.g, c.b, value);

		/// <summary> Returns a color with the hue replaced, keeping saturation, value and alpha. </summary>
		public static Color GetWithHue(this Color c, float hue)
		{
			Color.RGBToHSV(c, out _, out var s, out var v);

			var result = Color.HSVToRGB(hue, s, v);
			result.a = c.a;

			return result;
		}

		/// <summary> Returns a color with the saturation replaced, keeping hue, value and alpha. </summary>
		public static Color GetWithSaturation(this Color c, float saturation)
		{
			Color.RGBToHSV(c, out var h, out _, out var v);

			var result = Color.HSVToRGB(h, saturation, v);
			result.a = c.a;

			return result;
		}

		/// <summary> Returns a color with the value (brightness) replaced, keeping hue, saturation and alpha. </summary>
		public static Color GetWithValue(this Color c, float value)
		{
			Color.RGBToHSV(c, out var h, out var s, out _);

			var result = Color.HSVToRGB(h, s, value);
			result.a = c.a;

			return result;
		}

		/// <summary> Returns color hex string representation, in RGB or RGBA order. </summary>
		public static string GetHexValue(this Color32 c, bool isNeededAlpha = false)
		{
			var result = "#" + c.r.ToString(Format) + c.g.ToString(Format) + c.b.ToString(Format);

			if (isNeededAlpha)
				result += c.a.ToString(Format);

			return result;
		}

		/// <summary> Returns color hex string representation, in RGB or RGBA order. </summary>
		public static string GetHexValue(this Color c, bool isNeededAlpha = false) => ((Color32)c).GetHexValue(isNeededAlpha);

		/// <summary> Parses a color from a hex string ("RRGGBB" or "RRGGBBAA", with or without a leading '#'). Alpha defaults to opaque if omitted. </summary>
		public static Color32 FromHex(string hex)
		{
			if (hex == null)
				throw new ArgumentNullException(nameof(hex));

			if (hex.StartsWith("#"))
				hex = hex.Substring(1);

			if (hex.Length != 6 && hex.Length != 8)
				throw new ArgumentException($"Hex color string must be 6 or 8 characters long (RRGGBB or RRGGBBAA), got '{hex}'.", nameof(hex));

			var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
			var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
			var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
			var a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber) : byte.MaxValue;

			return new Color32(r, g, b, a);
		}
	}
}
