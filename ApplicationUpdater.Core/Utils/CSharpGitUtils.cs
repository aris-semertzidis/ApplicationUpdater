using System.Diagnostics;

public static class CSharpGitUtils
{
    /// <summary>
    /// Simple wrapper for executing commands.
    /// </summary>
    public static string? RunCommandLineCommand(string workingDirectory, string commandName, string arguments)
    {
        ProcessStartInfo processStartInfo = new()
        {
            WorkingDirectory = workingDirectory,
            FileName = commandName,
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using Process? process = Process.Start(processStartInfo);
        if (process == null)
            throw new Exception($"Failed to start process:{commandName}");

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    private static string? ExecuteGitCommand(string gitDirectory, string arguments)
    {
        return RunCommandLineCommand(gitDirectory, "git", arguments);
    }

    /// <summary>
    /// Find root git directory from a given folder.
    /// Iterate recursively upwards (parents).
    /// </summary>
    public static string? GetGitRootDirectory(string baseFolder)
    {
        if (!Directory.Exists(baseFolder))
            throw new DirectoryNotFoundException($"The folder does not exist: {baseFolder}");

        DirectoryInfo? currentDirectory = new DirectoryInfo(baseFolder);

        int maxTries = 20;
        while (currentDirectory != null)
        {
            if (Directory.Exists($"{currentDirectory}/.git"))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;

            if (maxTries-- < 0)
                break;
        }

        return null;
    }

    /// <summary>
    /// Get the short code of a commit (first 7 characters).
    /// </summary>
    public static string? GetCurrentBranch(string gitDirectory)
    {
        return ExecuteGitCommand(gitDirectory, "branch --show-current");
    }

    /// <summary>
    /// Get the latest commit hash (full) on current branch.
    /// </summary>
    public static string? GetLastCommitMessageName(string gitDirectory)
    {
        return RunCommandLineCommand(gitDirectory, "git", "log -n 1 --pretty=format:\"%B\"");
    }

    /// <summary>
    /// Get the latest commit hash (full) on current branch.
    /// </summary>
    public static string? GetLastCommitHashLong(string gitDirectory)
    {
        return ExecuteGitCommand(gitDirectory, "log -n 1 --pretty=format:\"%H\"");
    }

    /// <summary>
    /// Get the short code of a commit (first 7 characters).
    /// </summary>
    public static string? GetLastCommitHashShort(string gitDirectory)
    {
        return ExecuteGitCommand(gitDirectory, "log -n 1 --pretty=format:\"%h\"");
    }

    /// <summary>
    /// Get the short code of a commit (first 7 characters).
    /// </summary>
    public static string? GetLastCommitAuthorName(string gitDirectory)
    {
        return ExecuteGitCommand(gitDirectory, "log -n 1 --pretty=format:\"%ai\"");
    }
}