using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using NSubstitute;

namespace Nkolex.Propman.Tests
{
    public class EmailConfirmationMigratorTests
    {
        [Fact]
        public async Task Given_Account_With_Unconfirmed_Email_MigrateAsync_Should_Confirm_Account()
        {
            var account = new Account
            {
                Email = "test@example.com",
                EmailConfirmed = false,
                EmailConfirmationToken = "some-token",
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            var accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            accountDataService.GetAllAsync().Returns(new List<IAccount> { account });

            var migrator = new EmailConfirmationMigrator(accountDataService);
            await migrator.MigrateAsync();

            await accountDataService.Received(1).UpdateAsync(Arg.Is<IAccount>(a =>
                a.Email == account.Email &&
                a.EmailConfirmed &&
                a.EmailConfirmationToken == null &&
                a.EmailConfirmationTokenExpiresAt == null));
        }

        [Fact]
        public async Task Given_Account_With_Confirmed_Email_MigrateAsync_Should_Not_Update_Account()
        {
            var account = new Account { Email = "test@example.com", EmailConfirmed = true };

            var accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            accountDataService.GetAllAsync().Returns(new List<IAccount> { account });

            var migrator = new EmailConfirmationMigrator(accountDataService);
            await migrator.MigrateAsync();

            await accountDataService.DidNotReceive().UpdateAsync(Arg.Any<IAccount>());
        }
    }
}
