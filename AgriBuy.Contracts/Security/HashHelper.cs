using System;
using System.Security.Cryptography;
using System.Text;

namespace AgriBuy.Contracts.Security
{
    public class HashHelper
    {
        public string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        public bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            string hashedInput = HashPassword(inputPassword);
            return hashedInput == hashedPassword;
        }
    }
}

