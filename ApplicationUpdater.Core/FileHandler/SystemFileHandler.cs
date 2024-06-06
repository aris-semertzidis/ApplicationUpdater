using ApplicationUpdater.Core.Utils;

namespace ApplicationUpdater.Core.FileHandler;

public class SystemFileHandler : IFileWriter, IFileLoader
{
    public event EventHandlerT<int, int>? onProgress;
    public event EventHandlerT<string>? onStatusUpdate;

    #region IFileWriter

    public async Task WriteFiles(BuildManifest manifest, string sourcePath, string destinationPath, string manifestName)
    {
        // Clear any previous files in build
        if (Directory.Exists(destinationPath))
            IOUtils.DeleteFolder(destinationPath);

        Directory.CreateDirectory(destinationPath);

        // Push write to another thread
        await Task.Run(() =>
        {
            for (int i = 0; i < manifest.items.Length; i++)
            {
                string sourceFilename = PathUtils.CombinePaths(sourcePath, manifest.items[i].fileName);
                string destinationFileName = $"{destinationPath}{manifest.items[i].fileName}";
                if (!File.Exists(sourceFilename))
                {
                    throw new FileNotFoundException($"The file does not exist: {sourceFilename}");
                }

                onStatusUpdate?.Invoke($"Copying file:{manifest.items[i].fileName}");
                onProgress?.Invoke(i, manifest.items.Length);
                IOUtils.CopyFile(sourceFilename, destinationFileName);
            }

            string sourceManifestFilename = Path.Combine(sourcePath, manifestName);
            string destinationManifestFilename = Path.Combine(destinationPath, manifestName);
            onStatusUpdate?.Invoke($"Copying manifest:{sourceManifestFilename}");
            IOUtils.CopyFile(sourceManifestFilename, destinationManifestFilename);
        });
    }

    #endregion

    #region IFileLoader

    public Task<BuildManifest> LoadManifest(string buildPath, string manifestName)
    {
        string buildManifestSourcePath = PathUtils.CombinePaths(buildPath, manifestName);
        if (!File.Exists(buildManifestSourcePath))
            throw new FileNotFoundException($"The build manifest does not exist: {buildManifestSourcePath}");

        string buildManifestJson = File.ReadAllText(buildManifestSourcePath);
        BuildManifest buildManifest = JsonWrapper.Deserialize<BuildManifest>(buildManifestJson);
        return Task.FromResult(buildManifest);
    }

    public Task LoadFiles(BuildManifest buildManifest, string buildPath, string destinationPath)
    {
        for (int i = 0; i < buildManifest.items.Length; i++)
        {
            BuildManifest.ManifestItem item = buildManifest.items[i];
            CopyFile(buildPath, destinationPath, item.fileName);
        }

        onStatusUpdate?.Invoke($"Finished copying all files to:{destinationPath}");
        return Task.CompletedTask;
    }

    private void CopyFile(string buildPath, string destinationPath, string fileName)
    {
        onStatusUpdate?.Invoke($"Copying file:{fileName}");

        string sourceFilePath = PathUtils.CombinePaths(buildPath, fileName);
        string destinationFilePath = PathUtils.CombinePaths(destinationPath, fileName);

        IOUtils.CopyFile(sourceFilePath, destinationFilePath);
    }

    #endregion

}
