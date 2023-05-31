using UnityEngine;

namespace InsaneOne.Core
{
    public static class ColorExtensions
    {
        public static Color32 SetAlpha(this Color32 color, byte value)
        {
            color = new Color32(color.r, color.g, color.b, value);
			
            return color;
        }

        public static Color SetAlpha(this Color color, float value)
        {
            color = new Color(color.r, color.g, color.b, value);
			
            return color;
        }

        public static string GetHexValue(this Color32 inputColor, bool isNeededAlpha = false)
        {
            var result = "#";

            result += inputColor.r.ToString("X2") + inputColor.g.ToString("X2") + inputColor.b.ToString("X2");

            if (isNeededAlpha)
                result += inputColor.a.ToString("X2");

            return result;
        }

        public static string GetHexValue(this Color inputColor, bool isNeededAlpha = false)
        {
            Color32 color32 = inputColor;

            return color32.GetHexValue();
        }
    }
}