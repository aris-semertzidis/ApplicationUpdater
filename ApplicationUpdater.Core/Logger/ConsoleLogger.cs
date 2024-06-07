namespace ApplicationUpdater.Core.Logger;

public class ConsoleLogger : ILogger
{
    public void Log(string message, LogType logType = LogType.Info)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]:[{logType}] - {message}");
    }
}
