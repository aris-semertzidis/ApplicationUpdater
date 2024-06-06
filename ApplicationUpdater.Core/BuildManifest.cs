using System.Text.Json.Serialization;

namespace ApplicationUpdater.Core;

public class BuildManifest
{
    public record ManifestItem(string fileName, string hash);

    [JsonInclude]
    public readonly ManifestItem[] items;

    [JsonInclude]
    public string? version;

    [JsonInclude]
    public string? commit;

    public BuildManifest(int itemCount)
    {
        items = new ManifestItem[itemCount];
    }
}