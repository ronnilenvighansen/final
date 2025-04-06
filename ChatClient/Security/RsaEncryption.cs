using System.Security.Cryptography;
using System.Text;

namespace SharedSecurity
{
    public class RSAEncryption
    {
        private static RSA rsa = RSA.Create();

        public static string GetPublicKey() => Convert.ToBase64String(rsa.ExportRSAPublicKey());

        public static string EncryptWithPublicKey(string data, string publicKey)
        {
            using RSA rsaReceiver = RSA.Create();
            rsaReceiver.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            return Convert.ToBase64String(rsaReceiver.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.OaepSHA256));
        }

        public static string DecryptWithPrivateKey(string encryptedData)
        {
            byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedData), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}