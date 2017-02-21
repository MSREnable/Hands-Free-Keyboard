using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.HandsFree.Helpers.Telemetry
{
    public class CipherUtility
    {
        public static void DeriveKeyAndIv<T>(string password, string salt, out byte[] key, out byte[] iv, Encoding encoding)
            where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, encoding.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            key = rgb.GetBytes(algorithm.KeySize >> 3);
            iv = rgb.GetBytes(algorithm.BlockSize >> 3);
        }

        public static string Encrypt<T>(string plaintext, string password, string salt, Encoding encoding)
            where T : SymmetricAlgorithm, new()
        {
            byte[] key;
            byte[] iv;

            DeriveKeyAndIv<T>(password, salt, out key, out iv, encoding);

            return Encrypt<T>(plaintext, key, iv, encoding);
        }

        public static string Encrypt<T>(string plaintext, byte[] key, byte[] iv, Encoding encoding)
            where T : SymmetricAlgorithm, new()
        {
            SymmetricAlgorithm algorithm = new T();

            var transform = algorithm.CreateEncryptor(key, iv);

            var buffer = new MemoryStream();
            var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write);
            using (var writer = new StreamWriter(stream, encoding))
            {
                writer.Write(plaintext);
            }

            var ciphertext = Convert.ToBase64String(buffer.ToArray());
            return ciphertext;
        }

        public static string Decrypt<T>(string ciphertext, string password, string salt, Encoding encoding)
            where T : SymmetricAlgorithm, new()
        {
            byte[] key;
            byte[] iv;

            DeriveKeyAndIv<T>(password, salt, out key, out iv, encoding);

            return Decrypt<T>(ciphertext, key, iv, encoding);
        }

        public static string Decrypt<T>(string ciphertext, byte[] key, byte[] iv, Encoding encoding)
           where T : SymmetricAlgorithm, new()
        {
            SymmetricAlgorithm algorithm = new T();

            var transform = algorithm.CreateDecryptor(key, iv);

            string plaintext;

            var buffer = new MemoryStream(Convert.FromBase64String(ciphertext));
            var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read);
            using (var reader = new StreamReader(stream, encoding))
            {
                plaintext = reader.ReadToEnd();
            }

            return plaintext;
        }
    }
}
