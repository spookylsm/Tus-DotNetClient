namespace TusDotNetClientTests;

using System.Linq;

public class Utils
{
    public static string SHA1(byte[] bytes)
    {
        using (var sha1 = System.Security.Cryptography.SHA1.Create())
        {
            return string.Join(
                "",
                sha1.ComputeHash(bytes)
                    .Select(b => b.ToString("x2")));
        }
    }
}