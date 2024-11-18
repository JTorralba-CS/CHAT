//OK

using System.Security.Cryptography;
using System.Text;

namespace Portal.Services
{
    public class CryptoService
    {
        private readonly byte[] IV =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };

        private static byte[] DeriveKeyFromPassword(string password)
        {
            byte[]? emptySalt = Array.Empty<byte>();

            int iterations = 1024;

            int desiredKeyLength = 16;

            HashAlgorithmName hashMethod = HashAlgorithmName.SHA384;

            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.Unicode.GetBytes(password),
                emptySalt,
                iterations,
                hashMethod,
                desiredKeyLength);
        }
        
        public async Task<byte[]?> Encrypt(string unencrypted, string passphrase)
        {
            using Aes? aes = Aes.Create();

            aes.Key = DeriveKeyFromPassword(passphrase);
            aes.IV = IV;

            using MemoryStream? outputStream = new();
            using CryptoStream? cryptoStream = new(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(unencrypted));
            await cryptoStream.FlushFinalBlockAsync();

            return outputStream.ToArray();
        }

        public async Task<string?> Decrypt(byte[] encrypted, string passphrase)
        {
            using Aes? aes = Aes.Create();

            aes.Key = DeriveKeyFromPassword(passphrase);
            aes.IV = IV;

            using MemoryStream? inputStream = new(encrypted);
            using CryptoStream? cryptoStream = new(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream? outputStream = new();
            
            await cryptoStream.CopyToAsync(outputStream);

            return Encoding.Unicode.GetString(outputStream.ToArray());
        }

        public static byte[]? StringToByte(string x)
        {
            string[]? xStringArray = x.Split('-');

            byte[]? xByteArray = new byte[xStringArray.Length];

            for (int i = 0; i < xStringArray.Length; i++) xByteArray[i] = Convert.ToByte(xStringArray[i], 16);

            return xByteArray;
        }
    }
}
