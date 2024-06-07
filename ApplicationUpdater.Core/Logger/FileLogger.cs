namespace ApplicationUpdater.Core.Logger;

public class FileLogger : ILogger
{
    private readonly string logFilePath;

    public FileLogger(string logFilePath)
    {
        this.logFilePath = logFilePath;
    }

    public void Log(string message, LogType logType = LogType.Info)
    {
        string text = $"[{DateTime.Now:HH:mm:ss}]:[{logType}] - {message}\n";
        File.AppendAllText(logFilePath, text);
    }
}
