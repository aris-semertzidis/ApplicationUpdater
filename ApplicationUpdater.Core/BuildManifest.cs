namespace ApplicationUpdater.Core;

public class BuildManifest
{
    public record ManifestItem(string fileName, string hash);

    public ManifestItem[] items = Array.Empty<ManifestItem>();

    public string? version;

    public string? commit;

    /// <summary>
    /// Used for Json Serialization
    /// </summary>
    public BuildManifest() { }

    public BuildManifest(int itemCount)
    {
        items = new ManifestItem[itemCount];
    }
}