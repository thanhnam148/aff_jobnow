﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Aff.Services.Security
{
    public class SecurityUtil
    {
        private static string GetRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstvwxyz";
            var random = new Random();
            var randomString = new string(
                Enumerable.Repeat(chars, 32)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return randomString;
        }

        public static string GenerateRandomPassword(out string password)
        {
            password = GetRandomString();
            if (password.Length > 10)
            {
                password = password.Substring(0, 10);
            }
            var provider = new SHA1CryptoServiceProvider();
            var encoding = new UnicodeEncoding();
            var result = provider.ComputeHash(encoding.GetBytes(password));

            return Convert.ToBase64String(result);
        }

        public static string HashPassword(string password)
        {
            var provider = new SHA1CryptoServiceProvider();
            var encoding = new UnicodeEncoding();
            var result = provider.ComputeHash(encoding.GetBytes(password));

            return Convert.ToBase64String(result);
        }

        public static string GenerateToken(string sample)
        {
            var randomMasterPassword = GetRandomString();
            var crypto = new TripleDESCryptoServiceProvider { Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };

            var keySize = crypto.LegalKeySizes.Last().MaxSize;
            var key = new byte[keySize / 8];
            var index = 0;
            var masterPassword = Encoding.UTF8.GetBytes(randomMasterPassword);
            while (index < key.Length)
                key[index] = masterPassword[index++ % (masterPassword.Length - 1)];
            byte[] masterKey = key;

            crypto = new TripleDESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                Key = masterKey
            };

            ICryptoTransform cTransform = crypto.CreateEncryptor();
            var data = Encoding.UTF8.GetBytes(sample);
            byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
            crypto.Clear();

            var base64StringToken = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            var result = base64StringToken.Replace("+", "").Replace("/", "").Replace("=", "");
            return result;

        }


        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static string EncryptText(string input, string password = "E6t187^D43%F")
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        public static string DecryptText(string input, string password = "E6t187^D43%F")
        {
            // Get the bytes of the string
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }
        
    }
}