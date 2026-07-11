namespace Nkolex.Propman.Server.Data.ConnectionOptions
{
    /// <summary>
    /// Configuration for the AES-GCM based encryption-at-rest service.
    /// The Key must be a base64 encoded 256-bit (32 byte) value.
    /// </summary>
    public class EncryptionOptions
    {
        public required string Key { get; set; }
    }
}
