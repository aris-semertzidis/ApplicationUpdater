using ApplicationUpdater.Core.Utils;
using System.Net;

namespace ApplicationUpdater.Core.Builder.DataWriter;

public class FTPConfig
{
    public string url { get; set; }
    public string username { get; set; }
    public string password { get; set; }

    /// <summary>
    /// Allow uploading files without file extensions.
    /// Some FTP servers or implementation may not allow it.
    /// </summary>
    public bool allowFilesWithoutExtensions { get; set; } = true;
    /// <summary>
    /// A temporal file extension for files not having an extension.
    /// </summary>
    public string? temporalFileExtension { get; set; }

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

public class FTPWriter : IDataWriter
{
    private readonly FTPConfig config;
    private readonly NetworkCredential networkCredentials;
    private string workingDirectory = "";

    public FTPWriter(FTPConfig config)
    {
        this.config = config;
        networkCredentials = new NetworkCredential(this.config.username, this.config.password);

        if (!config.allowFilesWithoutExtensions && string.IsNullOrEmpty(config.temporalFileExtension))
            throw new ArgumentException("Temporal file extension is required when not allowing files without extensions.");
    }

    private FtpWebRequest CreateFTPRequest(string remoteFile, string method)
    {
        string remotePath = "";
        if(!config.url.StartsWith("ftp://"))
            remotePath = "ftp://";

        remotePath = PathUtils.CombinePaths(remotePath, config.url, workingDirectory, remoteFile);

#pragma warning disable SYSLIB0014 // Type or member is obsolete
        FtpWebRequest webRequest = (FtpWebRequest)WebRequest.Create(remotePath);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        webRequest.Credentials = networkCredentials;
        webRequest.Method = method;

        return webRequest;
    }

    public void SetWorkingDirectory(string workingDirectory)
    {
        this.workingDirectory = workingDirectory;
    }

    public Task WriteFiles(BuildManifest manifest, string sourcePath, string workingDirectory, string manifestName, Action<string>? logCallback = null)
    {
        SetWorkingDirectory(workingDirectory);

        // Upload manifest file
        string sourceManifestPath = PathUtils.CombinePaths(sourcePath, manifestName);
        UploadFile(sourceManifestPath, manifestName, logCallback);

        foreach (BuildManifest.ManifestItem file in manifest.items)
        {
            // Relative remotePath of the file
            string sourceFilePath = PathUtils.CombinePaths(sourcePath, file.fileName);

            string remoteFile = file.fileName;
            // Remove directory ('/') prefix
            if (remoteFile.StartsWith('/'))
                remoteFile = remoteFile.Remove(0, 1);

            UploadFile(sourceFilePath, remoteFile, logCallback);
        }

        return Task.CompletedTask;
    }

    public void CreateDirectoryRecursively(string directory, Action<string>? logCallback = null)
    {
        string[] incrementalDirectories = PathUtils.GetIncrementalDirectoryPaths(directory);

        for (int i = 0; i < incrementalDirectories.Length; i++)
        {
            CreateDirectory(incrementalDirectories[i], logCallback);
        }
    }

    public void CreateDirectory(string directory, Action<string>? logCallback = null)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.MakeDirectory);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                logCallback?.Invoke(resp.StatusCode.ToString());
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
            logCallback?.Invoke($"Create directory:{directory} failed:{ex}");
        }
    }

    public void UploadFile(string localFilePath, string remoteFilePath, Action<string>? logCallback = null)
    {
        if (!File.Exists(localFilePath))
        {
            logCallback?.Invoke($"File does not exist: {localFilePath}");
            return;
        }

        // Check files without a file extension
        if (!Path.HasExtension(remoteFilePath) && !config.allowFilesWithoutExtensions)
            remoteFilePath = $"{remoteFilePath}.{config.temporalFileExtension}";

        CreateDirectoryRecursively(remoteFilePath, logCallback);

        FtpWebRequest webRequest = CreateFTPRequest(remoteFilePath, WebRequestMethods.Ftp.UploadFile);

        try
        {
            logCallback?.Invoke($"Uploading file: {remoteFilePath}");
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
            logCallback?.Invoke(status);
            logCallback?.Invoke($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, status:{status}, ex:{e}");
        }
        catch (Exception ex)
        {
            logCallback?.Invoke($"Uploading failed for local file:{localFilePath}, remote file:{remoteFilePath}, exception:{ex}");
        }
    }

    public void Delete(string directory, Action<string>? logCallback = null)
    {
        WebRequest webRequest = CreateFTPRequest(directory, WebRequestMethods.Ftp.DeleteFile);
        try
        {
            using (FtpWebResponse resp = (FtpWebResponse)webRequest.GetResponse())
            {
                logCallback?.Invoke(resp.StatusCode.ToString());
            }
        }
        catch (Exception ex)
        {
            logCallback?.Invoke($"Delete directory:{directory} failed:{ex}");
        }
    }

    public void Rename(string remoteFile, string remoteFileNewName, Action<string>? logCallback = null)
    {
        FtpWebRequest webRequest = CreateFTPRequest(remoteFile, WebRequestMethods.Ftp.Rename);
        webRequest.RenameTo = remoteFileNewName;
        webRequest.UseBinary = false;
        webRequest.UsePassive = true;

        try
        {
            FtpWebResponse renameResponse = (FtpWebResponse)webRequest.GetResponse();
            logCallback?.Invoke($"Renamed file:{remoteFile}, to {remoteFileNewName}");
        }
        catch (Exception ex)
        {
            logCallback?.Invoke($"Error renaming file:{ex}");
        }
    }
}