using System.Globalization;

namespace InsaneOne.Core.Utility
{
	public static class TimeUtility
	{
		static readonly CultureInfo cultureProvider = CultureInfo.InvariantCulture;

		/// <summary> Returns better formatted seconds (with milliseconds included). </summary>
		public static string FormatSecondsPrettier(float seconds)
		{
			return seconds switch
			{
				>= 10 => seconds.ToString("F0", cultureProvider),
				>= 1 => seconds.ToString("F1", cultureProvider),
				< 1 => seconds.ToString("F2", cultureProvider),
				_ => "wrong number!",
			};
		}

		public static string SecondsToHhMmSs(int totalSeconds)
		{
			switch (totalSeconds)
			{
				case < 60:
					return totalSeconds.ToString();
				case < 3600: // Less than 60 minutes - format as mm:ss
				{
					var minutes = totalSeconds / 60;
					var seconds = totalSeconds % 60;
					return $"{minutes:00}:{seconds:00}";
				}
				default:
				{
					var hours = totalSeconds / 3600;
					var minutes = (totalSeconds % 3600) / 60;
					var seconds = totalSeconds % 60;
					return $"{hours:00}:{minutes:00}:{seconds:00}";
				}
			}
		}
	}
}