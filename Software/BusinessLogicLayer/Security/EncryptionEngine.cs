using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Security
{
    public class EncryptionEngine : ICryptoService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iteration = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private static readonly byte[] VerifierLabel = Encoding.UTF8.GetBytes("PassNest.MasterPasswordVerifier.v1");

        public string GenerateSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));

        public byte[] DeriveKey(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), saltBytes, Iteration, Algorithm, KeySize);
        }

        public string HashPassword(string password, string salt)
        {
            var key = DeriveKey(password, salt);
            return Convert.ToBase64String(HMACSHA256.HashData(key, VerifierLabel));
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            var key = DeriveKey(password, salt);
            var expected = Convert.FromBase64String(hash);

            var matchesNewFormat = CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(key, VerifierLabel), expected);
            var matchesLegacyFormat = CryptographicOperations.FixedTimeEquals(key, expected);

            return matchesNewFormat | matchesLegacyFormat;
        }

        public string Encrypt(string plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText, byte[] key)
        {
            var fullBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;

            var ivLength = aes.BlockSize / 8;
            var iv = new byte[ivLength];
            var cipherBytes = new byte[fullBytes.Length - ivLength];
            Buffer.BlockCopy(fullBytes, 0, iv, 0, ivLength);
            Buffer.BlockCopy(fullBytes, ivLength, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var painBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(painBytes);
        }
    }
}
