using System;
using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public class CoreUnityLogger : ILogger
	{
		const string CorePrefix = "<b>[InsaneOne.Core]</b> ";

		public static CoreUnityLogger I => instance ??= new CoreUnityLogger();
		static CoreUnityLogger instance;

		public void Log(string text, LogLevel logLevel = LogLevel.Info)
		{
			if (CoreData.TryLoad(out var coreData) && coreData.SuppressLogs)
				return;

			text = CorePrefix + text;

			switch (logLevel)
			{
				case LogLevel.Info:
				case LogLevel.Verbose:
					Debug.Log(text);
					break;

				case LogLevel.Warning:
					Debug.LogWarning(text);
					break;

				case LogLevel.Error:
					Debug.LogError(text);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		public static void DisposeInstance() => instance = null;
	}
}