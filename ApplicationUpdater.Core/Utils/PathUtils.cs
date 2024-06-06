namespace ApplicationUpdater.Core.Utils;

public static class PathUtils
{
    public static string CombinePaths(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = paths[i].UnifyDirectorySeparators();
            if (paths[i].StartsWith("/"))
                paths[i] = paths[i].Remove(0, 1);
        }

        return Path.Combine(paths).UnifyDirectorySeparators();
    }

    /// <summary>
    /// Split all directories from a remotePath.
    /// </summary>
    public static string[] SplitDirectories(string path)
    {
        // Unify separators
        path = path.UnifyDirectorySeparators();
        string[] directories = path.Split('/');
        if (directories[directories.Length - 1].Contains('.'))
            return directories.AsSpan(0, directories.Length - 1).ToArray();
        return directories.Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }

    /// <summary>
    /// Split all directories incrementally.
    /// e.g. root/data/config ->
    /// 1. root
    /// 2. root/data
    /// 3. root/data/config
    /// </summary>
    public static string[] GetIncrementalDirectoryPaths(string path)
    {
        path = Path.GetDirectoryName(path)!;
        string[] directories = SplitDirectories(path);
        for (int i = 0; i < directories.Length; i++)
        {
            if (i == 0)
                continue;
            directories[i] = Path.Combine(directories[i - 1], directories[i]);
        }
        return directories.Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }
}
