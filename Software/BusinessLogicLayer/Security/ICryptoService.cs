using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Security
{
    public interface ICryptoService
    {
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool VerifyPassword(string password, string hash, string salt);
        byte[] DeriveKey(string password, string salt);
        string Encrypt(string plainText, byte[] key);
        string Decrypt(string cipherText, byte[] key);
    }
}
