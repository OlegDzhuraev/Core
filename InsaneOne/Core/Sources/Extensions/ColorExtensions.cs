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
		public static Color GetWithAlpha(this Color color, float value) => new(color.r, color.g, color.b, value);

		public static string GetHexValue(this Color32 c, bool isNeededAlpha = false)
		{
			var result = "#" + c.r.ToString(Format) + c.g.ToString(Format) + c.b.ToString(Format);

			if (isNeededAlpha)
				result += c.a.ToString(Format);

			return result;
		}

		public static string GetHexValue(this Color inputColor, bool isNeededAlpha = false) => ((Color32)inputColor).GetHexValue();
	}
}
