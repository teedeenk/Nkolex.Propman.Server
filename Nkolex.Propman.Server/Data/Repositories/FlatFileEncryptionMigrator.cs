using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data.Repositories
{
    public class FlatFileEncryptionMigrator
    {
        private readonly IEncryptionService _encryptionService;

        public FlatFileEncryptionMigrator(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public async Task MigrateAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var fileContent = await File.ReadAllTextAsync(filePath);

            if (IsAlreadyEncrypted(fileContent))
            {
                return;
            }

            var encrypted = _encryptionService.Encrypt(fileContent);
            await File.WriteAllTextAsync(filePath, encrypted);
        }

        private bool IsAlreadyEncrypted(string fileContent)
        {
            try
            {
                _encryptionService.Decrypt(fileContent);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
