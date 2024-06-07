namespace ApplicationUpdater.Core.FileHandler;

public interface IHaveProgressEvents
{
    event EventHandlerT<int, int> onProgress;
    event EventHandlerT<string> onStatusUpdate;
}

public interface IFileWriter : IHaveProgressEvents
{
    Task WriteFiles(BuildManifest manifest, string sourcePath, string destinationPath, string manifestName);
}

public interface IFileLoader : IHaveProgressEvents
{
    Task LoadFiles(BuildManifest buildManifest, string localBuildPath);
    Task<BuildManifest> LoadManifest(string manifestName);
}