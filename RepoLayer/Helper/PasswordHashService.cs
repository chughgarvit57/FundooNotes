using System;
using System.Security.Cryptography;

namespace RepoLayer.Helper
{
    public class PasswordHashService
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int HashSize = 32; // 256-bit hash
        private const int Iterations = 10000;

        // Method to hash a password with salt
        public string HashPassword(string userPass)
        {
            try
            {
                // Generate salt
                byte[] salt = new byte[SaltSize];
                RandomNumberGenerator.Fill(salt);

                // Generate hash
                using var pbkdf2 = new Rfc2898DeriveBytes(userPass, salt, Iterations, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Convert to Base64
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to hash password.", ex);
            }
        }

        // Method to verify password against stored hash
        public bool VerifyPassword(string userPass, string storedHashPass)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHashPass);

                if (hashBytes.Length != SaltSize + HashSize)
                    return false;

                // Extract salt from stored hash
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Recompute hash with provided password and extracted salt
                using var pbkdf2 = new Rfc2898DeriveBytes(userPass, salt, Iterations, HashAlgorithmName.SHA256);
                byte[] computedHash = pbkdf2.GetBytes(HashSize);

                // Compare byte by byte (in constant time)
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != computedHash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
