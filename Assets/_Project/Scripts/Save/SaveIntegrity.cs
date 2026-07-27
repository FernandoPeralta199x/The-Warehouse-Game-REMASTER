using System.Security.Cryptography;
using System.Text;

namespace TW08.Save
{
    public static class SaveIntegrity
    {
        public static string ComputeChecksum(string payload)
        {
            payload ??= string.Empty;
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            StringBuilder builder = new(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        public static bool IsValid(string payload, string checksum)
        {
            if (string.IsNullOrWhiteSpace(checksum))
            {
                return false;
            }

            return ComputeChecksum(payload) == checksum;
        }
    }
}
