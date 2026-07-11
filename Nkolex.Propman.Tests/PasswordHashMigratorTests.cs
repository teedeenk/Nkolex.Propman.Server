using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using NSubstitute;

namespace Nkolex.Propman.Tests
{
    public class PasswordHashMigratorTests
    {
        [Fact]
        public async Task Given_Account_With_Plaintext_Password_MigrateAsync_Should_Update_Account_With_Hashed_Password()
        {
            var passwordHasher = new Pbkdf2PasswordHasher();
            var account = new Account { Email = "test@example.com", Password = "Password123!" };

            var accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            accountDataService.GetAllAsync().Returns(new List<IAccount> { account });

            var migrator = new PasswordHashMigrator(accountDataService, passwordHasher);
            await migrator.MigrateAsync();

            await accountDataService.Received(1).UpdateAsync(Arg.Is<IAccount>(a =>
                a.Email == account.Email &&
                passwordHasher.IsHashed(a.Password) &&
                passwordHasher.VerifyPassword(a.Password, "Password123!")));
        }

        [Fact]
        public async Task Given_Account_With_Hashed_Password_MigrateAsync_Should_Not_Update_Account()
        {
            var passwordHasher = new Pbkdf2PasswordHasher();
            var account = new Account { Email = "test@example.com", Password = passwordHasher.HashPassword("Password123!") };

            var accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            accountDataService.GetAllAsync().Returns(new List<IAccount> { account });

            var migrator = new PasswordHashMigrator(accountDataService, passwordHasher);
            await migrator.MigrateAsync();

            await accountDataService.DidNotReceive().UpdateAsync(Arg.Any<IAccount>());
        }
    }
}

