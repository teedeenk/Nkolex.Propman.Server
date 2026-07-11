using Nkolex.Propman.Server.Services;

namespace Nkolex.Propman.Tests
{
    public class Pbkdf2PasswordHasherTests
    {
        [Fact]
        public void Given_Password_HashPassword_Then_VerifyPassword_Should_Return_True()
        {
            var hasher = new Pbkdf2PasswordHasher();
            var password = "Password123!";

            var hashedPassword = hasher.HashPassword(password);
            var result = hasher.VerifyPassword(hashedPassword, password);

            Assert.True(result);
        }

        [Fact]
        public void Given_Wrong_Password_VerifyPassword_Should_Return_False()
        {
            var hasher = new Pbkdf2PasswordHasher();
            var hashedPassword = hasher.HashPassword("Password123!");

            var result = hasher.VerifyPassword(hashedPassword, "WrongPassword!");

            Assert.False(result);
        }

        [Fact]
        public void Given_Same_Password_HashPassword_Should_Return_Different_Hash_Each_Time()
        {
            var hasher = new Pbkdf2PasswordHasher();
            var password = "Password123!";

            var hash1 = hasher.HashPassword(password);
            var hash2 = hasher.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Given_Hashed_Password_IsHashed_Should_Return_True()
        {
            var hasher = new Pbkdf2PasswordHasher();
            var hashedPassword = hasher.HashPassword("Password123!");

            var result = hasher.IsHashed(hashedPassword);

            Assert.True(result);
        }

        [Fact]
        public void Given_Plaintext_Password_IsHashed_Should_Return_False()
        {
            var hasher = new Pbkdf2PasswordHasher();

            var result = hasher.IsHashed("Password123!");

            Assert.False(result);
        }
    }
}
