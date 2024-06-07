using ApplicationUpdater.Core.FileHandler;
using ApplicationUpdater.Core.Utils;

namespace ApplicationUpdater.Core.Updater;

public class AppUpdater : IHaveProgressEvents
{
    public event EventHandlerT<int, int>? onProgress;
    public event EventHandlerT<string>? onStatusUpdate;

    private readonly IFileLoader dataLoader;

    public AppUpdater(IFileLoader dataLoader)
    {
        this.dataLoader = dataLoader;

        // Propagate events
        dataLoader.onStatusUpdate += (status) => onStatusUpdate?.Invoke(status);
        dataLoader.onProgress += (progress, total) => onProgress?.Invoke(progress, total);
    }

    public async Task UpdateApplication(string localBuildPath, string manifestName)
    {
        // Download them in a temp folder
        string tempPath = Path.Combine(Environment.CurrentDirectory, "Tmp");
        IOUtils.DeleteFolder(tempPath);

        BuildManifest buildManifest = await dataLoader.LoadManifest(manifestName);
        if (buildManifest == null)
            throw new Exception("Failed to load manifest");

        List<BuildManifest.ManifestItem> invalidFiles = AppFileValidator.ValidateFiles(buildManifest, localBuildPath);

        if (invalidFiles.Count > 0)
        {
            BuildManifest buildManifestDiff = new BuildManifest(invalidFiles.Count);
            buildManifestDiff.items = invalidFiles.ToArray();
            bool success = await DownloadFiles(dataLoader, buildManifestDiff, tempPath);

            onStatusUpdate?.Invoke("Moving temp files into application");
            IOUtils.CopyFolder(tempPath, localBuildPath, false);

            onStatusUpdate?.Invoke("Clearing up temp files");
            IOUtils.DeleteFolder(tempPath);
        }
        else
        {
            onStatusUpdate?.Invoke("All files are up to date");
        }

    }

    private async Task<bool> DownloadFiles(IFileLoader dataLoader, BuildManifest buildManifest, string destinationPath)
    {
        const int RETRY_COUNT = 3;
        int retries = 0;
        while (++retries < RETRY_COUNT)
        {
            await dataLoader.LoadFiles(buildManifest, destinationPath);

            List<BuildManifest.ManifestItem> invalidFiles = AppFileValidator.ValidateFiles(buildManifest, destinationPath);
            if (invalidFiles.Count == 0)
                break;

            buildManifest = new BuildManifest(invalidFiles.Count);
            buildManifest.items = invalidFiles.ToArray();
            onStatusUpdate?.Invoke("Invalid files detected. Redownloading...");
        }

        return retries < RETRY_COUNT;
    }
}
