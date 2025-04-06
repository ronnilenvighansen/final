using System.Security.Cryptography;

namespace ChatClient.Security
{
    public static class AesHmacEncryption
    {
        private static byte[] _key;

        public static void SetKey(string base64Key)
        {
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
                throw new CryptographicException("AES key size must be 16, 24, or 32 bytes.");
        }

        public static (string EncryptedData, string IV, string Hmac) Encrypt(string plainText)
        {
            if (_key == null)
                throw new InvalidOperationException("AES key is not set. Call SetKey first.");

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            byte[] iv = aes.IV;
            byte[] encrypted;

            using (MemoryStream memoryStream = new())
            using (CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(plainText);
                streamWriter.Flush();
                cryptoStream.FlushFinalBlock();
                encrypted = memoryStream.ToArray();
            }

            string base64Encrypted = Convert.ToBase64String(encrypted);
            string base64IV = Convert.ToBase64String(iv);

            byte[] hmac;
            using (HMACSHA256 hmacSha256 = new(_key))
            {
                byte[] combined = iv.Concat(encrypted).ToArray();
                hmac = hmacSha256.ComputeHash(combined);
            }

            string base64Hmac = Convert.ToBase64String(hmac);
            return (base64Encrypted, base64IV, base64Hmac);
        }

        public static string Decrypt(string base64CipherText, string base64iv, string base64Hmac)
        {
            if (_key == null)
                throw new InvalidOperationException("AES key is not set. Call SetKey first.");

            byte[] iv = Convert.FromBase64String(base64iv);
            byte[] cipherText = Convert.FromBase64String(base64CipherText);
            byte[] receivedHmac = Convert.FromBase64String(base64Hmac);

            using (HMACSHA256 hmacSha256 = new(_key))
            {
                byte[] combined = iv.Concat(cipherText).ToArray();
                byte[] computedHmac = hmacSha256.ComputeHash(combined);

                if (!computedHmac.SequenceEqual(receivedHmac))
                {
                    throw new CryptographicException("HMAC verification failed. Data may have been tampered with.");
                }
            }

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using MemoryStream memoryStream = new(cipherText);
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
