namespace Nkolex.Propman.Server.Abstractions
{
    /// <summary>
    /// Provides encryption/decryption of data at rest for storage providers
    /// (such as the flat-file repository) that do not offer their own
    /// built-in encryption-at-rest capability.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the given plain text and returns a payload safe to persist to disk.
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts a payload previously produced by <see cref="Encrypt"/>.
        /// </summary>
        string Decrypt(string cipherText);
    }
}
