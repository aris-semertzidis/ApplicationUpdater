namespace ApplicationUpdater.Core.Utils;

public static class IOUtils
{
    public static void DeleteFile(string path)
    {
        if (!File.Exists(path))
            return;
        File.Delete(path);
    }

    public static void DeleteFolder(string path)
    {
        if (!Directory.Exists(path))
            return;
        Directory.Delete(path, true);
    }

    public static void CopyFile(string sourcePath, string destinationPath)
    {
        // Check if source file exists
        if (!File.Exists(sourcePath))
            return;

        CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        // Copy file
        File.Copy(sourcePath, destinationPath, true);
    }

    public static void CopyFolder(string sourcePath, string destinationPath, bool deleteDestinationContents = false)
    {
        if (!Directory.Exists(sourcePath))
            return;

        if (deleteDestinationContents)
            DeleteFolder(destinationPath);

        CreateDirectory(destinationPath);

        // Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
        }

        // Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }
    }

    public static void CreateDirectory(string path)
    {
        // Check if destination directory exists
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static string? ReadTextFile(string path)
    {
        if (!File.Exists(path))
            return null;

        return File.ReadAllText(path);
    }
}
