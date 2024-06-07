using ApplicationUpdater.Core.Utils;

namespace ApplicationUpdater.Core.Updater;
public class AppFileValidator
{
    public static List<BuildManifest.ManifestItem> ValidateFiles(BuildManifest buildManifest, string localPath)
    {
        List<BuildManifest.ManifestItem> invalidFiles = new();
        foreach (BuildManifest.ManifestItem item in buildManifest.items)
        {
            string filePath = PathUtils.CombinePaths(localPath, item.fileName);
            if (!File.Exists(filePath))
            {
                invalidFiles.Add(item);
                continue;
            }

            string fileHash = ChecksumUtil.ChecksumString(filePath);
            if (fileHash != item.hash)
            {
                invalidFiles.Add(item);
            }
        }

        return invalidFiles;
    }

}
