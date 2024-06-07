namespace ApplicationUpdater.Core.Utils;

internal static class GenericUtils
{
    public static string UnifyDirectorySeparators(this string path) => path.Replace('\\', '/');
}
