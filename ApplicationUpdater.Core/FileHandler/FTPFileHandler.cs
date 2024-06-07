using ApplicationUpdater.Core.Logger;
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

public class FTPFileHandler : IFileWriter
{
    public event EventHandlerT<int, int>? onProgress;
    public event EventHandlerT<string>? onStatusUpdate;

    private readonly FTPConfig config;
    private readonly NetworkCredential networkCredentials;
    private readonly ILogger? logger;

    private string workingDirectory = "";

    public FTPFileHandler(FTPConfig config, ILogger? logger = null)
    {
        this.config = config;
        this.logger = logger;
        networkCredentials = new NetworkCredential(this.config.username, this.config.password);

        if (!config.allowFilesWithoutExtensions && string.IsNullOrEmpty(config.temporalFileExtension))
            throw new ArgumentException("Temporal file extension is required when not allowing files without extensions.");
    }

    public void SetWorkingDirectory(string workingDirectory)
    {
        this.workingDirectory = workingDirectory;
    }

    public async Task WriteFiles(BuildManifest manifest, string localBuildFolder, string workingDirectory, string manifestName)
    {
        SetWorkingDirectory(workingDirectory);

        // Upload manifest file
        string sourceManifestPath = PathUtils.CombinePaths(localBuildFolder, manifestName);
        await UploadFile(sourceManifestPath, manifestName);

        List<string> allFolders = GetAllDirectories(manifest);
        foreach (string allDirectories in allFolders)
        {
            await CreateDirectory(allDirectories);
        }

        for (int i = 0; i < manifest.items.Length; i++)
        {
            BuildManifest.ManifestItem file = manifest.items[i];
            // Relative prefix of the file
            string sourceFilePath = PathUtils.CombinePaths(localBuildFolder, file.fileName);

            string remoteFile = file.fileName;
            // Remove directory ('/') prefix
            if (remoteFile.StartsWith('/'))
                remoteFile = remoteFile.Remove(0, 1);

            await UploadFile(sourceFilePath, remoteFile);

            onProgress?.Invoke(i, manifest.items.Length);
        }
    }

    private static List<string> GetAllDirectories(BuildManifest buildManifest)
    {
        List<string> allDirectories = new List<string>();
        foreach (BuildManifest.ManifestItem item in buildManifest.items)
        {
            string[] incrementalDirectories = PathUtils.GetIncrementalDirectoryPaths(item.fileName);
            allDirectories.AddRange(incrementalDirectories);
        }

        return allDirectories.GroupBy(directory => directory).Select(directory => directory.First()).ToList();
    }

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

    private Task CreateDirectory(string directory)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.MakeDirectory);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                Log($"Creating directory:{directory} returned code:{resp.StatusCode}");
            }
        }
        catch (WebException ex)
        {
            // Directory already exists
            if (ex.Message.Contains("550"))
                return Task.CompletedTask;

            throw;
        }
        catch (Exception ex)
        {
            Log($"Create directory:{directory} failed:{ex}", LogType.Exception);
        }
        return Task.CompletedTask;
    }

    private Task UploadFile(string localFilePath, string remoteFilePath)
    {
        if (!File.Exists(localFilePath))
        {
            Log($"File does not exist: {localFilePath}", LogType.Error);
            return Task.CompletedTask;
        }

        // Check files without a file extension
        if (!Path.HasExtension(remoteFilePath) && !config.allowFilesWithoutExtensions)
            remoteFilePath = $"{remoteFilePath}.{config.temporalFileExtension}";

        CreateDirectoryRecursively(remoteFilePath);

        FtpWebRequest webRequest = CreateFTPRequest(remoteFilePath, WebRequestMethods.Ftp.UploadFile);

        try
        {
            Log($"Uploading file: {remoteFilePath}");
            using (Stream fileStream = File.OpenRead(localFilePath))
            {
                using (Stream ftpStream = webRequest.GetRequestStream())
                {
                    // TODO: Inject CancellationToken and use CopyToAsync
                    fileStream.CopyTo(ftpStream);
                }
            }
        }
        catch (WebException e)
        {
            string status = ((FtpWebResponse)e.Response!).StatusDescription!;
            Log($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, status:{status}, ex:{e}", LogType.Exception);
        }
        catch (Exception ex)
        {
            Log($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, exception:{ex}", LogType.Exception);
        }
        return Task.CompletedTask;
    }

    private void Delete(string directory)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.DeleteFile);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                Log($"Deleted file:{directory} returned code:{resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Log($"Delete directory:{directory} failed:{ex}", LogType.Exception);
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
            Log($"Renamed file:{remoteFile}, to {remoteFileNewName} returned code:{renameResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            Log($"Error renaming file{remoteFile} throw {ex}", LogType.Exception);
        }
    }

    #endregion
    private void Log(string message, LogType type = LogType.Info)
    {
        logger?.Log(message, type);
        if (type == LogType.Info)
            onStatusUpdate?.Invoke(message);
    }
}