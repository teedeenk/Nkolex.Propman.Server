using Nkolex.Propman.Server.Abstractions;
using System.Security.Cryptography;

namespace Nkolex.Propman.Server.Services
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSizeBytes);

            var result = new byte[SaltSizeBytes + HashSizeBytes];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSizeBytes);
            Buffer.BlockCopy(hash, 0, result, SaltSizeBytes, HashSizeBytes);

            return Convert.ToBase64String(result);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            ArgumentNullException.ThrowIfNull(hashedPassword);
            ArgumentNullException.ThrowIfNull(providedPassword);

            var data = Convert.FromBase64String(hashedPassword);
            if (data.Length != SaltSizeBytes + HashSizeBytes)
            {
                return false;
            }

            var salt = data.AsSpan(0, SaltSizeBytes).ToArray();
            var expectedHash = data.AsSpan(SaltSizeBytes, HashSizeBytes).ToArray();

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, Iterations, Algorithm, HashSizeBytes);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }

        public bool IsHashed(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            try
            {
                var data = Convert.FromBase64String(value);
                return data.Length == SaltSizeBytes + HashSizeBytes;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
