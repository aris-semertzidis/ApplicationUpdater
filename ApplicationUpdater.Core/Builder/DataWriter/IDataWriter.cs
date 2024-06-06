namespace ApplicationUpdater.Core.Builder.DataWriter;

public interface IDataWriter
{
    Task WriteFiles(BuildManifest manifest, string sourcePath, string destinationPath, string manifestName, Action<string>? logCallback = null);
}
