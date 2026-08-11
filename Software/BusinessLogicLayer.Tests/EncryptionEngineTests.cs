using BusinessLogicLayer.Security;
using System.Security.Cryptography;

namespace BusinessLogicLayer.Tests
{
    public class EncryptionEngineTests
    {
        private readonly EncryptionEngine crypto = new();

        [Fact]
        public void GenerateSalt_ReturnsDifferentValueEachTime()
        {
            var salt1 = crypto.GenerateSalt();
            var salt2 = crypto.GenerateSalt();

            Assert.NotEqual(salt1, salt2);
        }

        [Fact]
        public void HashPassword_SameInput_ProducesSameHash()
        {
            var salt = crypto.GenerateSalt();

            var hash1 = crypto.HashPassword("MojaLozinka123!", salt);
            var hash2 = crypto.HashPassword("MojaLozinka123!", salt);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var salt = crypto.GenerateSalt();
            var hash = crypto.HashPassword("MojaLozinka123!", salt);

            Assert.True(crypto.VerifyPassword("MojaLozinka123!", hash, salt));
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var salt = crypto.GenerateSalt();
            var hash = crypto.HashPassword("MojaLozinka123!", salt);

            Assert.False(crypto.VerifyPassword("KrivaLozinka", hash, salt));
        }

        [Fact]
        public void DeriveKey_SamePasswordAndSalt_ProducesSameKey()
        {
            var salt = crypto.GenerateSalt();

            var key1 = crypto.DeriveKey("GlavnaLozinka1!", salt);
            var key2 = crypto.DeriveKey("GlavnaLozinka1!", salt);

            Assert.Equal(key1, key2);
        }

        [Fact]
        public void DeriveKey_DifferentSalt_ProducesDifferentKey()
        {
            var key1 = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());
            var key2 = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void DeriveKey_ReturnsKeyOfExpected256BitLength()
        {
            var key = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());

            Assert.Equal(32, key.Length);
        }

        [Fact]
        public void EncryptDecrypt_RoundTrip_ReturnsOriginalPlainText()
        {
            var key = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());
            const string original = "korisnicka-lozinka-za-servis";

            var encrypted = crypto.Encrypt(original, key);
            var decrypted = crypto.Decrypt(encrypted, key);

            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Encrypt_SamePlainText_ProducesDifferentCipherTextEachTime()
        {
            var key = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());

            var cipher1 = crypto.Encrypt("ista-lozinka", key);
            var cipher2 = crypto.Encrypt("ista-lozinka", key);

            Assert.NotEqual(cipher1, cipher2);
        }

        [Fact]
        public void Decrypt_WithWrongKey_ThrowsOrProducesDifferentText()
        {
            var key = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());
            var wrongKey = crypto.DeriveKey("DrugaLozinka2!", crypto.GenerateSalt());
            const string original = "tajna-lozinka";

            var encrypted = crypto.Encrypt(original, key);

            try
            {
                var decrypted = crypto.Decrypt(encrypted, wrongKey);
                Assert.NotEqual(original, decrypted);
            }
            catch (CryptographicException)
            {
            }
        }
    }
}