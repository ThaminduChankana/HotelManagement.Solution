using System.Security.Cryptography;
using System.Text;

namespace UserService.Utils
{
    // Utility class for hashing and verifying passwords using PBKDF2
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 10000;
        private static readonly char Delimiter = ':';

        // Hashes a password using PBKDF2 with a random salt
        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);

            return Convert.ToBase64String(salt) + Delimiter + Convert.ToBase64String(key);
        }

        // Verifies a password against a hash
        public static bool Verify(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split(Delimiter);
                if (parts.Length != 2)
                    return false;

                var salt = Convert.FromBase64String(parts[0]);
                var key = Convert.FromBase64String(parts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var keyToCheck = pbkdf2.GetBytes(KeySize);

                return keyToCheck.SequenceEqual(key);
            }
            catch
            {
                return false;
            }
        }
    }
}