namespace ApplicationUpdater.Core.Logger;

public interface ILogger
{
    void Log(string message, LogType logType = LogType.Info);
}

public enum LogType
{
    Info,
    Warning,
    Error,
    Exception
}
