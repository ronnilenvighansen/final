using System.Security.Cryptography;
using System.Text;

namespace SharedSecurity
{
    public static class AesEncryption
    {
        private static byte[] _key;

        public static void SetKey(string base64Key)
        {
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
                throw new CryptographicException("AES key size must be 16, 24, or 32 bytes.");
        }
        public static (string EncryptedData, string IV) Encrypt(string plainText)
        {
            if (_key == null)
                throw new InvalidOperationException("AES key is not set. Call SetKey first.");

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using StreamWriter streamWriter = new(cryptoStream);
            
            streamWriter.Write(plainText);
            streamWriter.Flush();
            cryptoStream.FlushFinalBlock();

            string encryptedData = Convert.ToBase64String(memoryStream.ToArray());
            string ivString = Convert.ToBase64String(aes.IV); // Convert IV to Base64 for transmission
            
            return (encryptedData, ivString);        
        }

        public static string Decrypt(string cipherText, string base64IV)
        {
             if (_key == null)
                throw new InvalidOperationException("AES key is not set. Call SetKey first.");

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = Convert.FromBase64String(base64IV);

            byte[] buffer = Convert.FromBase64String(cipherText);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }

        public static string GenerateRandomKey()
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
    }
}