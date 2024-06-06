using ApplicationUpdater.Core.Builder.DataWriter;
using ApplicationUpdater.Core.Utils;
using System.Text.Json;

namespace ApplicationUpdater.Core.Builder;

public class ApplicationBuilder
{
    public static Task BuildApplicationWithLocalWriter(string folderPath, string destinationPath,
        string manifestName,
        string searchPattern = "*",
        string? excludePattern = null,
        Action<string>? logCallback = null)
    {
        IDataWriter dataWriter = new SystemFileWriter();
        return BuildApplication(folderPath, destinationPath, manifestName, dataWriter, searchPattern, excludePattern, logCallback);
    }

    public static Task BuildApplicationWithFTPWriter(string folderPath, string destinationPath,
        string manifestName,
        FTPConfig ftpParams,
        string searchPattern = "*",
        string? excludePattern = null,
        Action<string>? logCallback = null)
    {
        FTPWriter dataWriter = new FTPWriter(ftpParams);
        return BuildApplication(folderPath, destinationPath, manifestName, dataWriter, searchPattern, excludePattern, logCallback);
    }

    public static async Task BuildApplication(string folderPath, string destinationPath,
        string manifestName,
        IDataWriter dataWriter,
        string searchPattern = "*",
        string? excludePattern = null,
        Action<string>? logCallback = null)
    {
        folderPath = folderPath.UnifyDirectorySeparators();
        destinationPath = destinationPath.UnifyDirectorySeparators();

        logCallback?.Invoke("Creating manifest");

        // Create manifest for build
        BuildManifest manifest = CreateManifest(folderPath, manifestName, searchPattern, excludePattern);
        string commit = CreateCommitVersionFromGit(folderPath);
        // Create commit version
        manifest.commit = commit;

        // Save manifest locally
        string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(folderPath, manifestName), json);

        // Write all files in manifest to destination
        await dataWriter.WriteFiles(manifest, folderPath, destinationPath, manifestName, logCallback);
    }

    /// <summary>
    /// Create a manifest of all files in a folder.
    /// </summary>
    /// <param name="folderPath">The path to the folder containing the files.</param>
    /// <param name="includeFilesPattern">The search pattern to filter the files. Default is "*" (all files).</param>
    /// <param name="excludeFilesPattern">The search pattern to exclude specific file types from the manifest.</param>
    /// <returns>A BuildManifest object containing all files in folder and their checksums (MD5).</returns>
    /// <exception cref="ArgumentNullException">Thrown when the folderPath parameter is null or empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the folderPath does not exist.</exception>
    public static BuildManifest CreateManifest(string? folderPath, string manifestName, string includeFilesPattern = "*", string? excludeFilesPattern = null)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The folder does not exist: {folderPath}");

        // Delete already existing manifest if any
        IOUtils.DeleteFile(PathUtils.CombinePaths(folderPath, manifestName));

        // Read all files in the folder
        string[] allFiles = Directory.GetFiles(folderPath, includeFilesPattern, SearchOption.AllDirectories);

        // Exclude files if any set
        if (!string.IsNullOrEmpty(excludeFilesPattern))
            allFiles = allFiles.Where(file => !Path.GetFileName(file).EndsWith(excludeFilesPattern)).ToArray();

        BuildManifest builder = new(allFiles.Length);

        for (int i = 0; i < allFiles.Length; i++)
        {
            string fullFilename = allFiles[i];
            string hash = ChecksumUtil.ChecksumString(fullFilename);

            // Get the relative path of the file
            string partialFilename = fullFilename.Substring(folderPath.Length).Replace("\\", "/");

            builder.items[i] = new BuildManifest.ManifestItem(partialFilename, hash);
        }

        return builder;
    }

    /// <summary>
    /// Create a commit version string based on the current state of a Git repository.
    /// </summary>
    /// <param name="gitBaseFolder">The base folder of the Git repository.</param>
    /// <returns>A string representing the commit version in the format "branch-commit".</returns>
    public static string CreateCommitVersionFromGit(string gitBaseFolder)
    {
        string gitRoot = CSharpGitUtils.GetGitRootDirectory(gitBaseFolder)!;
        string commit = CSharpGitUtils.GetLastCommitHashShort(gitRoot)!;
        string branch = CSharpGitUtils.GetCurrentBranch(gitRoot)!.Replace("\n", "");

        return $"{branch}:{commit}";
    }
}
