using System.Text.Json;

namespace ApplicationUpdater.Core.Utils;
public static class JsonWrapper
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true });
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true })!;
    }
}
