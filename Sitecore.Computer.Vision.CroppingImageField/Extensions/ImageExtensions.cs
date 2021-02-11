using System.Linq;

namespace Sitecore.Computer.Vision.CroppingImageField.Extensions
{
    public static class ImageExtensions
    {
        public static string GetHashKey(this byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }
    }
}