using ApplicationUpdater.Core.Logger;
using ApplicationUpdater.Core.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ApplicationUpdater.Core.FileHandler;

public class HttpConfig
{
    public required string url;

    /// <summary>
    /// Allow uploading files without file extensions.
    /// Some FTP servers or implementation may not allow it.
    /// </summary>
    public bool allowFilesWithoutExtensions = true;
    /// <summary>
    /// A temporal file extension for files not having an extension.
    /// </summary>
    public string? temporalFileExtension;

    public int concurrentDownloads = 1;
}

public class HttpClientFileHandler : IFileLoader
{
    public event EventHandlerT<int, int>? onProgress;
    public event EventHandlerT<string>? onStatusUpdate;

    private readonly HttpConfig config;
    private readonly ILogger? logger;

    public HttpClientFileHandler(HttpConfig config, ILogger? logger = null)
    {
        this.config = config;
        this.logger = logger;
        if (!config.allowFilesWithoutExtensions && string.IsNullOrEmpty(config.temporalFileExtension))
            throw new ArgumentException("Temporal file extension is required when not allowing files without extensions.");
    }

    public async Task<BuildManifest> LoadManifest(string manifestName)
    {
        string remotePath = PathUtils.CombinePaths(config.url, manifestName);
        string buildManifestJson = await DownloadTextFileToMemory(remotePath);
        return JsonWrapper.Deserialize<BuildManifest>(buildManifestJson)!;
    }

    public async Task LoadFiles(BuildManifest buildManifest, string localBuildPath)
    {
        List<Task> concurrentTasks = new List<Task>(config.concurrentDownloads);
        for (int i = 0; i < buildManifest.items.Length; i++)
        {
            BuildManifest.ManifestItem item = buildManifest.items[i];
            string itemName = item.fileName;

            // If configuration not allowing files without extensions, find the file having the temporal extension
            if (!Path.HasExtension(item.fileName) && !config.allowFilesWithoutExtensions)
                itemName = $"{item.fileName}.{config.temporalFileExtension}";

            string sourceFilePath = PathUtils.CombinePaths(config.url, itemName);

            // Destination file path is without the temporal extension
            string destinationFilePath = PathUtils.CombinePaths(localBuildPath, item.fileName);

            Log($"Downloading {item.fileName}");
            Task downloadFileTask = DownloadFile(destinationFilePath, sourceFilePath);

            concurrentTasks.Add(downloadFileTask);
            if (concurrentTasks.Count >= config.concurrentDownloads)
            {
                await Task.WhenAll(concurrentTasks);
                concurrentTasks.Clear();
            }
            onProgress?.Invoke(i, buildManifest.items.Length);
        }

        Log("Finished downloading all files");
    }

    /// <summary>
    /// Download a file from the remote server to memory and parse it as string.
    /// </summary>
    private async Task<string> DownloadTextFileToMemory(string remoteFilePath)
    {
        Log($"Downloading {remoteFilePath}");
        using Stream stream = await GetRemoteFileStream(remoteFilePath);
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    /// <summary>
    /// Download a file from the remote server to local system.
    /// </summary>
    private async Task DownloadFile(string localFilePath, string remoteFilePath)
    {
        // Create directory for file
        IOUtils.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

        try
        {
            using Stream stream = await GetRemoteFileStream(remoteFilePath);
            using FileStream fileStream = new(localFilePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            Log($"There was an error downloading file:{remoteFilePath} to local path:{localFilePath}. ex:{ex}", LogType.Exception);
        }
    }

    private static async Task<Stream> GetRemoteFileStream(string remoteFilePath)
    {
        // Check if there is already a http or https prefix
        // TODO: Add config for forcing https
        if (!remoteFilePath.StartsWith("http"))
            remoteFilePath = "http://" + remoteFilePath;

        using HttpClient client = new();
        return await client.GetStreamAsync(remoteFilePath);
    }

    private void Log(string message, LogType type = LogType.Info)
    {
        logger?.Log(message, type);
        if (type == LogType.Info)
            onStatusUpdate?.Invoke(message);
    }
}