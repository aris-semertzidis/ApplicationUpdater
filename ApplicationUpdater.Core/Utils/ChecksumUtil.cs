using System.Security.Cryptography;

namespace ApplicationUpdater.Core.Utils;

public class ChecksumUtil
{
    public static byte[] ChecksumBytes(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                return md5.ComputeHash(stream);
            }
        }
    }

    public static string ChecksumString(string filename)
    {
        byte[] checksumBytes = ChecksumBytes(filename);
        return BitConverter.ToString(checksumBytes).Replace("-", "").ToLowerInvariant();
    }
}
