using System.Collections;
using System.Text;

namespace ApplicationUpdater.Core;

public enum SystemTargetEnum
{
    LocalFileSystem = 0,
    FTP = 1,
    // Add more system targets here
}

public class SystemTargetEnumerable : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        foreach (SystemTargetEnum target in Enum.GetValues(typeof(SystemTargetEnum)))
        {
            yield return target;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (object? item in this)
        {
            sb.Append("(");
            sb.Append((int)(SystemTargetEnum)item);
            sb.Append(")");
            sb.Append(item);
            sb.Append(", ");
        }
        sb.Remove(sb.Length - 2, 1);
        return sb.ToString();
    }
}