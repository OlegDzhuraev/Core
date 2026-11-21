using System;
using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public class CoreUnityLogger : ILogger
	{
		const string CorePrefix = "<b>[InsaneOne.Core]</b> ";

		/// <summary> Unique instance of the logger. </summary>
		public static CoreUnityLogger I => instance ??= new CoreUnityLogger();
		static CoreUnityLogger instance;

		public void Log(string text, LogLevel logLevel = LogLevel.Info) => Log(text, logLevel, null);

		/// <summary> Logs the message and highlights the context object when the log line is clicked in the Console. </summary>
		public void Log(string text, LogLevel logLevel, UnityEngine.Object context)
		{
			if (CoreData.TryLoad(out var coreData) && coreData.SuppressLogs)
				return;

			text = CorePrefix + text;

			switch (logLevel)
			{
				case LogLevel.Info:
				case LogLevel.Verbose:
					Debug.Log(text, context);
					break;

				case LogLevel.Warning:
					Debug.LogWarning(text, context);
					break;

				case LogLevel.Error:
					Debug.LogError(text, context);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		public static void DisposeInstance() => instance = null;
	}
}