using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Data.Repositories;
using Nkolex.Propman.Server.Services;

namespace Nkolex.Propman.Tests
{
    public class FlatFileEncryptionMigratorTests : IDisposable
    {
        private readonly string _filePath;
        private readonly AesGcmEncryptionService _encryptionService;

        public FlatFileEncryptionMigratorTests()
        {
            _filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
            var options = Options.Create(new EncryptionOptions { Key = "V4aOISDhX3LbGYQjphnIQ+6LS5fiRz9lyY3F4J7KD9s=" });
            _encryptionService = new AesGcmEncryptionService(options);
        }

        public void Dispose()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        [Fact]
        public async Task Given_PlainText_File_MigrateAsync_Should_Encrypt_File_Content()
        {
            var plainText = "{\"Account\":[]}";
            await File.WriteAllTextAsync(_filePath, plainText);

            var migrator = new FlatFileEncryptionMigrator(_encryptionService);
            await migrator.MigrateAsync(_filePath);

            var fileContentAfterMigration = await File.ReadAllTextAsync(_filePath);
            var decrypted = _encryptionService.Decrypt(fileContentAfterMigration);

            Assert.Equal(plainText, decrypted);
        }

        [Fact]
        public async Task Given_Already_Encrypted_File_MigrateAsync_Should_Not_Change_File_Content()
        {
            var plainText = "{\"Account\":[]}";
            var alreadyEncrypted = _encryptionService.Encrypt(plainText);
            await File.WriteAllTextAsync(_filePath, alreadyEncrypted);

            var migrator = new FlatFileEncryptionMigrator(_encryptionService);
            await migrator.MigrateAsync(_filePath);

            var fileContentAfterMigration = await File.ReadAllTextAsync(_filePath);
            var decrypted = _encryptionService.Decrypt(fileContentAfterMigration);

            Assert.Equal(plainText, decrypted);
        }

        [Fact]
        public async Task Given_File_Does_Not_Exist_MigrateAsync_Should_Do_Nothing()
        {
            var migrator = new FlatFileEncryptionMigrator(_encryptionService);

            await migrator.MigrateAsync(_filePath);

            Assert.False(File.Exists(_filePath));
        }
    }
}
