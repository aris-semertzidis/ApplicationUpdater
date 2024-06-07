using System.Text.Json;

namespace ApplicationUpdater.Core.Utils;

public static class JsonWrapper
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, jsonOptions);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, jsonOptions)!;
    }
}
