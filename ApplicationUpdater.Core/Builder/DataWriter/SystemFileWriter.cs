using ApplicationUpdater.Core.Utils;

namespace ApplicationUpdater.Core.Builder.DataWriter;

public class SystemFileWriter : IDataWriter
{
    public async Task WriteFiles(BuildManifest manifest, string sourcePath, string destinationPath, string manifestName, Action<string>? logCallback = null)
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
                logCallback?.Invoke($"Copying file:{manifest.items[i].fileName}");
                string sourceFilename = PathUtils.CombinePaths(sourcePath, manifest.items[i].fileName);
                string destinationFileName = $"{destinationPath}{manifest.items[i].fileName}";
                if (!File.Exists(sourceFilename))
                {
                    throw new FileNotFoundException($"The file does not exist: {sourceFilename}");
                }

                IOUtils.CopyFile(sourceFilename, destinationFileName);
            }

            string sourceManifestFilename = Path.Combine(sourcePath, manifestName);
            string destinationManifestFilename = Path.Combine(destinationPath, manifestName);
            logCallback?.Invoke($"Copying manifest:{sourceManifestFilename}");
            IOUtils.CopyFile(sourceManifestFilename, destinationManifestFilename);
        });
    }
}
