using ApplicationUpdater.Core.Utils;
using System.Net;
using System.Text;

namespace ApplicationUpdater.Core.FileHandler;

public class FTPConfig
{
    public string url;
    public string username;
    public string password;

    /// <summary>
    /// Allow uploading files without file extensions.
    /// Some FTP servers or implementation may not allow it.
    /// </summary>
    public bool allowFilesWithoutExtensions = true;
    /// <summary>
    /// A temporal file extension for files not having an extension.
    /// </summary>
    public string? temporalFileExtension;

    /// <summary>
    /// Used for Json serialization.
    /// </summary>
    public FTPConfig()
    {
        url = "";
        username = "";
        password = "";
    }

    public FTPConfig(string url, string username, string password)
    {
        this.url = url;
        this.username = username;
        this.password = password;
    }

    public FTPConfig(string url, string username, string password, bool allowFilesWithoutExtensions, string temporalFileExtension)
        : this(url, username, password)
    {
        this.allowFilesWithoutExtensions = allowFilesWithoutExtensions;
        this.temporalFileExtension = temporalFileExtension;
    }
}

public class FTPFileHandler : IFileWriter, IFileLoader
{
    public event EventHandlerT<int, int>? onProgress;
    public event EventHandlerT<string>? onStatusUpdate;

    private readonly FTPConfig config;
    private readonly NetworkCredential networkCredentials;
    private string workingDirectory = "";

    public FTPFileHandler(FTPConfig config)
    {
        this.config = config;
        networkCredentials = new NetworkCredential(this.config.username, this.config.password);

        if (!config.allowFilesWithoutExtensions && string.IsNullOrEmpty(config.temporalFileExtension))
            throw new ArgumentException("Temporal file extension is required when not allowing files without extensions.");
    }

    public void SetWorkingDirectory(string workingDirectory)
    {
        this.workingDirectory = workingDirectory;
    }

    public Task WriteFiles(BuildManifest manifest, string sourcePath, string workingDirectory, string manifestName)
    {
        SetWorkingDirectory(workingDirectory);

        // Upload manifest file
        string sourceManifestPath = PathUtils.CombinePaths(sourcePath, manifestName);
        UploadFile(sourceManifestPath, manifestName);

        foreach (BuildManifest.ManifestItem file in manifest.items)
        {
            // Relative prefix of the file
            string sourceFilePath = PathUtils.CombinePaths(sourcePath, file.fileName);

            string remoteFile = file.fileName;
            // Remove directory ('/') prefix
            if (remoteFile.StartsWith('/'))
                remoteFile = remoteFile.Remove(0, 1);

            UploadFile(sourceFilePath, remoteFile);
        }

        return Task.CompletedTask;
    }

    public async Task<BuildManifest> LoadManifest(string buildPath, string manifestName)
    {
        string buildManifestJson = await DownloadTextFileToMemory(PathUtils.CombinePaths(buildPath, manifestName));
        return JsonWrapper.Deserialize<BuildManifest>(buildManifestJson);
    }

    public async Task LoadFiles(BuildManifest buildManifest, string buildPath, string destinationPath)
    {
        Task[] tasks = new Task[buildManifest.items.Length];
        for (int i = 0; i < buildManifest.items.Length; i++)
        {
            BuildManifest.ManifestItem item = buildManifest.items[i];
            string itemName = item.fileName;

            // If configuration not allowing files without extensions, find the file having the temporal extension
            if (!Path.HasExtension(item.fileName) && !config.allowFilesWithoutExtensions)
                itemName = $"{item.fileName}.{config.temporalFileExtension}";

            string sourceFilePath = PathUtils.CombinePaths(buildPath, itemName);

            // Destination file path is without the temporal extension
            string destinationFilePath = PathUtils.CombinePaths(destinationPath, item.fileName);
            Task downloadTask = DownloadFile(destinationFilePath, sourceFilePath);
            tasks[i] = downloadTask;
        }

        await Task.WhenAll(tasks);
        onStatusUpdate?.Invoke("Finished downloading all files");
    }

    #region Download Files

    /// <summary>
    /// Download a file from the remote server to memory and parse it as string.
    /// </summary>
    private async Task<string> DownloadTextFileToMemory(string remoteFilePath)
    {
        onStatusUpdate?.Invoke($"Downloading {remoteFilePath}");
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
        onStatusUpdate?.Invoke($"Downloading {remoteFilePath} to {localFilePath}");

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
            onStatusUpdate?.Invoke($"There was an error downloading file:{remoteFilePath} to local path:{localFilePath}. ex:{ex}");
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

    #endregion

    #region FTP Functions

    private FtpWebRequest CreateFTPRequest(string remoteFile, string method)
    {
        string prefix = "";
        if (!config.url.StartsWith("ftp://"))
            prefix = "ftp://";
        string remotePath = PathUtils.CombinePaths(prefix, config.url, workingDirectory, remoteFile);

#pragma warning disable SYSLIB0014 // Type or member is obsolete
        FtpWebRequest webRequest = (FtpWebRequest)WebRequest.Create(remotePath);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        webRequest.Credentials = networkCredentials;
        webRequest.Method = method;

        return webRequest;
    }

    private void CreateDirectoryRecursively(string directory)
    {
        string[] incrementalDirectories = PathUtils.GetIncrementalDirectoryPaths(directory);

        for (int i = 0; i < incrementalDirectories.Length; i++)
        {
            CreateDirectory(incrementalDirectories[i]);
        }
    }

    private void CreateDirectory(string directory)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.MakeDirectory);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                onStatusUpdate?.Invoke(resp.StatusCode.ToString());
            }
        }
        catch (WebException ex)
        {
            // Directory already exists
            if (ex.Message.Contains("550"))
                return;

            throw;
        }
        catch (Exception ex)
        {
            onStatusUpdate?.Invoke($"Create directory:{directory} failed:{ex}");
        }
    }

    private void UploadFile(string localFilePath, string remoteFilePath)
    {
        if (!File.Exists(localFilePath))
        {
            onStatusUpdate?.Invoke($"File does not exist: {localFilePath}");
            return;
        }

        // Check files without a file extension
        if (!Path.HasExtension(remoteFilePath) && !config.allowFilesWithoutExtensions)
            remoteFilePath = $"{remoteFilePath}.{config.temporalFileExtension}";

        CreateDirectoryRecursively(remoteFilePath);

        FtpWebRequest webRequest = CreateFTPRequest(remoteFilePath, WebRequestMethods.Ftp.UploadFile);

        try
        {
            onStatusUpdate?.Invoke($"Uploading file: {remoteFilePath}");
            using (Stream fileStream = File.OpenRead(localFilePath))
            {
                using (Stream ftpStream = webRequest.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }
            }
        }
        catch (WebException e)
        {
            string status = ((FtpWebResponse)e.Response!).StatusDescription!;
            onStatusUpdate?.Invoke(status);
            onStatusUpdate?.Invoke($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, status:{status}, ex:{e}");
        }
        catch (Exception ex)
        {
            onStatusUpdate?.Invoke($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, exception:{ex}");
        }
    }

    private void Delete(string directory)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.DeleteFile);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                onStatusUpdate?.Invoke(resp.StatusCode.ToString());
            }
        }
        catch (Exception ex)
        {
            onStatusUpdate?.Invoke($"Delete directory:{directory} failed:{ex}");
        }
    }

    private void Rename(string remoteFile, string remoteFileNewName)
    {
        FtpWebRequest webRequest = CreateFTPRequest(remoteFile, WebRequestMethods.Ftp.Rename);
        webRequest.RenameTo = remoteFileNewName;
        webRequest.UseBinary = false;
        webRequest.UsePassive = true;

        try
        {
            FtpWebResponse renameResponse = (FtpWebResponse)webRequest.GetResponse();
            onStatusUpdate?.Invoke($"Renamed file:{remoteFile}, to {remoteFileNewName}");
        }
        catch (Exception ex)
        {
            onStatusUpdate?.Invoke($"Error renaming file:{ex}");
        }
    }

    #endregion
}