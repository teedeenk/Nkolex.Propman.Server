using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Services;

namespace Nkolex.Propman.Tests
{
    public class AesGcmEncryptionServiceTests
    {
        private static AesGcmEncryptionService CreateService(string key = "V4aOISDhX3LbGYQjphnIQ+6LS5fiRz9lyY3F4J7KD9s=")
        {
            var options = Options.Create(new EncryptionOptions { Key = key });
            return new AesGcmEncryptionService(options);
        }

        [Fact]
        public void Given_PlainText_Encrypt_Then_Decrypt_Should_Return_Original_Text()
        {
            var service = CreateService();
            var plainText = "Sensitive property data";

            var cipherText = service.Encrypt(plainText);
            var result = service.Decrypt(cipherText);

            Assert.Equal(plainText, result);
        }

        [Fact]
        public void Given_Same_PlainText_Encrypt_Should_Return_Different_CipherText_Each_Time()
        {
            var service = CreateService();
            var plainText = "Sensitive property data";

            var cipherText1 = service.Encrypt(plainText);
            var cipherText2 = service.Encrypt(plainText);

            Assert.NotEqual(cipherText1, cipherText2);
        }

        [Fact]
        public void Given_CipherText_Decrypt_With_Wrong_Key_Should_Throw()
        {
            var encryptingService = CreateService();
            var decryptingService = CreateService("Z9pxVhQwmnZ5r4oOgxwsOAxJ3vGRQO2Bh0X2K7yj+CQ=");
            var cipherText = encryptingService.Encrypt("Sensitive property data");

            Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() => decryptingService.Decrypt(cipherText));
        }

        [Fact]
        public void Given_Empty_Key_Constructor_Should_Throw_ArgumentException()
        {
            var options = Options.Create(new EncryptionOptions { Key = string.Empty });

            Assert.Throws<ArgumentException>(() => new AesGcmEncryptionService(options));
        }

        [Fact]
        public void Given_Key_Of_Wrong_Length_Constructor_Should_Throw_ArgumentException()
        {
            var shortKey = Convert.ToBase64String(new byte[10]);
            var options = Options.Create(new EncryptionOptions { Key = shortKey });

            Assert.Throws<ArgumentException>(() => new AesGcmEncryptionService(options));
        }
    }
}
