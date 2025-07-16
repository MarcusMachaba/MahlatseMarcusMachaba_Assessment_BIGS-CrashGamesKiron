using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Utilities
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) CreateHash(string password);
        bool VerifyPassword(string password, string hash, string salt);
    }


    public class PasswordHasher : IPasswordHasher
    {
        public (string Hash, string Salt) CreateHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            return (hash, salt);
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                throw new ArgumentException("Password, hash, and salt cannot be null or empty.");
            var computedHash = HashPassword(password, salt);
            return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        private string GenerateSalt()
        {
            // Implement a method to generate a secure random salt
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private string HashPassword(string password, string salt)
        {
            // Implement a secure hashing algorithm (e.g., PBKDF2, bcrypt)
            using var hasher = System.Security.Cryptography.SHA256.Create();
            var saltedPassword = $"{password}{salt}";
            var hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
