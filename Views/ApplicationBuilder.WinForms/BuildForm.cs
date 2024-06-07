using ApplicationUpdater.Core.Builder;
using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Logger;
using ApplicationUpdater.Core.Utils;
using ApplicationUpdater.WinForms;

namespace ApplicationBuilder.WinForms;

public partial class BuildForm : Form
{
    private const string FTP_CONFIG_PATH = "ftp.json";
    private const string MANIFEST_NAME = "manifest.json";
    private const string APP_SETTINGS_PATH = "app_settings.json";

    private AppSettings appSettings = new();

    public BuildForm()
    {
        InitializeComponent();
        ThreadRunner.Initialize();
        FormClosed += BuildForm_FormClosed;
        LoadAppSettings();
    }

    private void BuildForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        ThreadRunner.Dispose();
        SaveAppSettings();
    }

    private async void Build()
    {
        if (!File.Exists(FTP_CONFIG_PATH))
        {
            statusLabel.Text = $"FTP config is not found: {FTP_CONFIG_PATH}";
            return;
        }

        string text = File.ReadAllText(FTP_CONFIG_PATH);
        FTPConfig? ftpConfig = JsonWrapper.Deserialize<FTPConfig>(text);
        if (ftpConfig == null)
        {
            statusLabel.Text = $"FTP config is not valid.";
            return;
        }

        ILogger logger = new FileLogger(Path.Combine(Environment.CurrentDirectory, "log.txt"));
        IFileWriter dataWriter = new FTPFileHandler(ftpConfig, logger);

        string sourcePath = appSettings.localBuildPath!;
        string workingDirectory = appSettings.workingDirectory!;

        AppBuilder appBuilder = new(dataWriter);
        appBuilder.onProgress += AppBuilder_onProgress;
        appBuilder.onStatusUpdate += AppBuilder_onStatusUpdate;

        await Task.Run(async () =>
        {
            await appBuilder.BuildApplication(
                sourcePath,
                workingDirectory,
                MANIFEST_NAME);

            ThreadRunner.Run(() =>
            {
                statusLabel.Text = $"Build uploaded to:{ftpConfig.url}/{workingDirectory}";
                progressBar1.Value = 100;
                buildButton.Enabled = true;
            });
        });
    }

    private void AppBuilder_onStatusUpdate(string value)
    {
        ThreadRunner.Run(() =>
        {
            statusLabel.Text = value;
        });
    }

    private void AppBuilder_onProgress(int value1, int value2)
    {
        float percent = (float)value1 / (float)value2;
        ThreadRunner.Run(() =>
        {
            progressBar1.Value = (int)(percent * 100);
        });
    }

    private void buildButton_Click(object sender, EventArgs e)
    {
        buildButton.Enabled = false;
        SaveAppSettings();
        Build();
    }

    private void SaveAppSettings()
    {
        appSettings.localBuildPath = localBuildTextBox.Text;
        appSettings.workingDirectory = workingDirectoryTextBox.Text;

        string text = JsonWrapper.Serialize(appSettings);
        File.WriteAllText(APP_SETTINGS_PATH, text);
    }

    private void LoadAppSettings()
    {
        if (!File.Exists(APP_SETTINGS_PATH))
        {
            appSettings = new AppSettings();
            return;
        }

        string text = File.ReadAllText(APP_SETTINGS_PATH);
        appSettings = JsonWrapper.Deserialize<AppSettings>(text) ?? new AppSettings();

        localBuildTextBox.Text = appSettings.localBuildPath;
        workingDirectoryTextBox.Text = appSettings.workingDirectory;
    }
}

public class AppSettings
{
    public string? localBuildPath;
    public string? workingDirectory;
}
