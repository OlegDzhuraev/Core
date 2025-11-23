namespace InsaneOne.Core.Utility
{
	public interface ILogger
	{
		public void Log(string text, LogLevel logLevel);
	}

	public enum LogLevel
	{
		Verbose,
		Info,
		Warning,
		Error,
	}
}