using UnityEngine;

namespace InsaneOne.Core.Utility
{
	public interface ILogger
	{
		public void Log(string text, LogLevel logLevel);

		/// <summary> Same as Log, but also highlights the passed Unity object when the log message is clicked in the Console. </summary>
		public void Log(string text, LogLevel logLevel, Object context) => Log(text, logLevel);
	}

	public enum LogLevel
	{
		Verbose,
		Info,
		Warning,
		Error,
	}
}