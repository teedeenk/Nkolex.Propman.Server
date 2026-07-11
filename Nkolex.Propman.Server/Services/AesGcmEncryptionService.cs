using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using System.Security.Cryptography;

namespace Nkolex.Propman.Server.Services
{
    public class AesGcmEncryptionService : IEncryptionService
    {
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;

        private readonly byte[] _key;

        public AesGcmEncryptionService(IOptions<EncryptionOptions> options)
        {
            var keyValue = options.Value.Key;
            if (string.IsNullOrWhiteSpace(keyValue))
            {
                throw new ArgumentException("Encryption Key must be provided in EncryptionOptions.");
            }

            _key = Convert.FromBase64String(keyValue);
            if (_key.Length is not (16 or 24 or 32))
            {
                throw new ArgumentException("Encryption Key must decode to 16, 24, or 32 bytes.");
            }
        }

        public string Encrypt(string plainText)
        {
            ArgumentNullException.ThrowIfNull(plainText);

            var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[TagSizeBytes];

            using (var aesGcm = new AesGcm(_key, TagSizeBytes))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            var result = new byte[NonceSizeBytes + cipherBytes.Length + TagSizeBytes];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
            Buffer.BlockCopy(cipherBytes, 0, result, NonceSizeBytes, cipherBytes.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSizeBytes + cipherBytes.Length, TagSizeBytes);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            ArgumentNullException.ThrowIfNull(cipherText);

            var data = Convert.FromBase64String(cipherText);
            if (data.Length < NonceSizeBytes + TagSizeBytes)
            {
                throw new CryptographicException("Cipher text is too short to be valid.");
            }

            var nonce = data.AsSpan(0, NonceSizeBytes);
            var tag = data.AsSpan(data.Length - TagSizeBytes, TagSizeBytes);
            var cipherBytes = data.AsSpan(NonceSizeBytes, data.Length - NonceSizeBytes - TagSizeBytes);
            var plainBytes = new byte[cipherBytes.Length];

            using (var aesGcm = new AesGcm(_key, TagSizeBytes))
            {
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return System.Text.Encoding.UTF8.GetString(plainBytes);
        }
    }
}
