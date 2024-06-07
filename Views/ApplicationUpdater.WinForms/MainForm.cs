using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Logger;
using ApplicationUpdater.Core.Updater;
using ApplicationUpdater.Core.Utils;
using System.Diagnostics;

namespace ApplicationUpdater.WinForms;

public partial class MainForm : Form
{
    private const string MANIFEST_NAME = "manifest.json";
    private const string HTTP_CONFIG_PATH = "http.json";
    private const string APP_CONFIG_PATH = "app.json";

    public MainForm()
    {
        InitializeComponent();
        ThreadRunner.Initialize();
        UpdateApplication();
        FormClosed += (sender, e) => ThreadRunner.Dispose();
    }

    public async void UpdateApplication()
    {
        try
        {

            ILogger logger = new FileLogger(Path.Combine(Environment.CurrentDirectory, "log.txt"));

            string localAppPath = Path.Combine(Environment.CurrentDirectory, "PushArena");

            AppConfig? appConfig = LoadJsonFromFile<AppConfig>(APP_CONFIG_PATH);
            this.Text = appConfig.appName;

            HttpConfig? httpConfig = LoadJsonFromFile<HttpConfig>(HTTP_CONFIG_PATH);
            IFileLoader dataLoader = new HttpClientFileHandler(httpConfig, logger);

            AppUpdater appUpdater = new(dataLoader);
            appUpdater.onStatusUpdate += AppUpdater_onStatusUpdate;
            appUpdater.onProgress += AppUpdater_onProgress;
            await Task.Run(async () =>
            {
                await appUpdater.UpdateApplication(
                localAppPath,
                MANIFEST_NAME);
            });

            progressLabel.Text = "Application is up to date";
            progressBar.Value = 100;

            await Task.Delay(100);
            if (appConfig.executableToRunAfterUpdate != null)
            {
                string fullExecutablePath = Path.Combine(localAppPath, appConfig.executableToRunAfterUpdate);
                Process.Start(fullExecutablePath);
            }
            Close();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void AppUpdater_onProgress(int value1, int value2)
    {
        float percent = (float)value1 / (float)value2;
        ThreadRunner.Run(() =>
        {
            progressBar.Value = (int)(percent * 100);
        });
    }

    private void AppUpdater_onStatusUpdate(string value)
    {
        ThreadRunner.Run(() =>
        {
            progressLabel.Text = value;
        });
    }

    private static T LoadJsonFromFile<T>(string file)
    {
        string text = File.ReadAllText(file);
        T? config = JsonWrapper.Deserialize<T>(text);
        if (config == null)
            throw new Exception("FTP config is not valid.");
        return config;
    }

    public class AppConfig
    {
        public string? appName = null;
        public string? executableToRunAfterUpdate = null;
    }
}
