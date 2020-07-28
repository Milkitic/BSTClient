using BSTClient.Shared.Credentials;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BSTClient.Shared
{
    public static class UserPasswordManager
    {
        private static string ApplicationName => AppDomain.CurrentDomain.FriendlyName;

        public static bool TryGet(out string username, out string password)
        {
            try
            {
                var result = CredentialManager.ReadCredential(ApplicationName);
                if (result == null)
                {
                    username = default;
                    password = default;
                    return false;
                }

                username = result.UserName;
                password = Unprotect(result.Password);
                return true;
            }
            catch (Exception ex)
            {
                username = default;
                password = default;
                return false;
            }
        }

        public static void Set(string username, string password)
        {
            CredentialManager.WriteCredential(ApplicationName, username, Protect(password), CredentialPersistence.LocalMachine);
        }

        public static void Delete()
        {
            CredentialManager.DeleteCredential(ApplicationName);
        }

        private static string Protect(string str)
        {
            byte[] encrypted;
            using (var algorithm = Aes.Create())
            {
                encrypted = Encrypt(str, algorithm.Key, algorithm.IV);
                SaveKey(algorithm.Key);
                SaveIv(algorithm.IV);
            }

            byte[] entropy = Encoding.Unicode.GetBytes(ApplicationName);
            string protectedData = Convert.ToBase64String(ProtectedData.Protect(encrypted, entropy, DataProtectionScope.CurrentUser));
            return protectedData;
        }

        private static string Unprotect(string str)
        {
            // Encrypt the string to an array of bytes.
            byte[] entropy = Encoding.Unicode.GetBytes(ApplicationName);
            byte[] protectedData = Convert.FromBase64String(str);
            //string data = Encoding.Unicode.GetString(ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser));

            //var bytes = Encoding.Unicode.GetBytes(data);
            var bytes = ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser);
            var readKey = ReadKey();
            var readIv = ReadIv();
            string decrypted = Decrypt(bytes, readKey, readIv);
            return decrypted;
        }

        private static byte[] ReadKey()
        {
            var pass = CredentialManager.ReadCredential($"{ApplicationName}/token/a")?.Password;
            return Encoding.Unicode.GetBytes(pass);
        }

        private static byte[] ReadIv()
        {
            var pass = CredentialManager.ReadCredential($"{ApplicationName}/token/b")?.Password;
            return Encoding.Unicode.GetBytes(pass);
        }

        private static void SaveKey(byte[] bytes)
        {
            CredentialManager.WriteCredential($"{ApplicationName}/token/a", "token",
                Encoding.Unicode.GetString(bytes),
                CredentialPersistence.LocalMachine);
        }

        private static void SaveIv(byte[] bytes)
        {
            CredentialManager.WriteCredential($"{ApplicationName}/token/b", "token",
                Encoding.Unicode.GetString(bytes),
                CredentialPersistence.LocalMachine);
        }
        private static byte[] Encrypt(string text, byte[] deKey, byte[] deIv)
        {
            var ms = new MemoryStream();
            var key = Aes.Create(); //SymmetricAlgorithm
            key.Padding = PaddingMode.PKCS7;

            var encStream = new CryptoStream(ms, key.CreateEncryptor(deKey, deIv), CryptoStreamMode.Write);
            var sw = new StreamWriter(encStream);

            sw.WriteLine(text);
            sw.Close();
            encStream.Close();

            byte[] buffer = ms.ToArray();
            ms.Close();

            return buffer;
        }

        private static string Decrypt(byte[] encrypted, byte[] deKey, byte[] deIv)
        {
            var ms = new MemoryStream(encrypted);
            var key = Aes.Create();
            key.Padding = PaddingMode.PKCS7;

            var encStream = new CryptoStream(ms, key.CreateDecryptor(deKey, deIv), CryptoStreamMode.Read);
            var sr = new StreamReader(encStream);

            string val = sr.ReadLine();

            sr.Close();
            encStream.Close();
            ms.Close();

            return val;
        }
    }
}
