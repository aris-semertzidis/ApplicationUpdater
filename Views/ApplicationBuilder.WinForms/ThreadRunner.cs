using Timer = System.Windows.Forms.Timer;

namespace ApplicationUpdater.WinForms;

internal static class ThreadRunner
{
    private static readonly Timer timer = new Timer();
    private static readonly Queue<Action> actions = new();

    public static void Initialize()
    {
        timer.Interval = (int)Math.Floor(1f / 30f * 1000);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    public static void Dispose()
    {
        timer.Dispose();
    }

    private static void Timer_Tick(object? sender, EventArgs e)
    {
        while (actions.Count > 0)
        {
            Action action = actions.Dequeue();
            action();
        }
    }

    public static void Run(Action action)
    {
        actions.Enqueue(action);
    }

}
