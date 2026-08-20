using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using NSubstitute;
using System.Security.Cryptography;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]

    public class AccountDataServiceTests : TestFixture
    {
        private readonly IAccountDataService<IAccount> _accountDataService;
        private IRepository<IAccount> _repo;

        public AccountDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _accountDataService = Factory.Services.GetRequiredService<IAccountDataService<IAccount>>();
            _repo = Factory.Services.GetRequiredService<IRepository<IAccount>>();
        }

        [Fact]
        public async Task Given_CreateAccount_AddAsync_Should_Return_1()
        {
            var createAccount = CreateTestAccount();
            var sud = await _accountDataService.AddAsync(createAccount);
            Assert.Equal(1,sud);
        }

        [Fact]
        public async Task Given_There_Are_Accounts_GetAllAsync_Should_Return_All_Accounts()
        {
            var accounts = CreateTestAccountList();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetAllAsync().Returns(accounts);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, _repo);

            var sud = await accountService.GetAllAsync();

            Assert.NotNull(sud);
            Assert.Equal(accounts, sud);
        }

        [Fact]
        public async Task Given_valid_email_GetByIdAsync_Should_Return_Account()
        {
            var account = CreateTestAccount();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetAllAsync().Returns([account]);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, _repo);

            var sud = await accountService.GetByIdAsync(account);

            Assert.NotNull(sud);
            Assert.Equal(account.Email, sud.Email);
        }

        [Fact]

        public async Task Given_ValidRole_Should_Be_Able_To_Update_User_Role() 
        {
            var account = CreateTestAccount();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.UpdateAsync(account).Returns(1);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, _repo);

            var sud = await accountService.UpdateAsync(account);

            Assert.Equal(1, sud);
        }

        [Fact]
        public async Task Given_ValidToken_GetByEmailConfirmationTokenAsync_Should_Return_Account()
        {
            var account = CreateTestAccount();
            account.EmailConfirmationToken = Guid.NewGuid().ToString("N");

            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetAllAsync().Returns([account]);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, _repo);

            var result = await accountService.GetByEmailConfirmationTokenAsync(account.EmailConfirmationToken);

            Assert.NotNull(result);
            Assert.Equal(account.Email, result.Email);
            Assert.Equal(account.EmailConfirmationToken, result.EmailConfirmationToken);
        }

        [Fact]
        public async Task Given_InvalidToken_GetByEmailConfirmationTokenAsync_Should_Not_Match()
        {
            var account = CreateTestAccount();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetAllAsync().Returns([account]);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Factory.Services;

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, _repo);

            var result = await accountService.GetByEmailConfirmationTokenAsync("invalid-token");

            Assert.NotNull(result);
            // When token is not found, it returns a default account instance
            Assert.NotEqual(account.Email, result.Email);
        }

        [Fact]
        public async Task Given_NoAccounts_GetByEmailConfirmationTokenAsync_Should_Return_Default()
        {
            var accounts = new List<IAccount>();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetAllAsync().Returns(accounts);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var accountService = new AccountDataService<IAccount>(Factory.Services, logger, _repo);

            var result = await accountService.GetByEmailConfirmationTokenAsync("any-token");

            Assert.NotNull(result);
            // When no accounts exist and token not found, returns default account
        }

        [Fact]
        public async Task Given_Account_DeleteAsync_Should_Delete_Account()
        {
            var account = CreateTestAccount();
            var substitutedRepo = Substitute.For<IRepository<IAccount>>();
            substitutedRepo.DeleteAsync(account).Returns(1);

            var logger = Substitute.For<ILogger<AccountDataService<IAccount>>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService<IAccount>(serviceProvider, logger, substitutedRepo);

            var result = await accountService.DeleteAsync(account);
            Assert.Equal(1, result);
            await substitutedRepo.Received(1).DeleteAsync(account);
        }

        private IAccount CreateTestAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Id = Guid.NewGuid();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = $"john{RandomNumberGenerator.GetInt32(1, 20_000)}.doe@example.com";
            account.Password = "TestPassword123!";
            account.AgreeToTerms = true;
            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            account.DeletedAt = null;
            account.IsDeleted = false;
            return account;
        }

        private List<IAccount> CreateTestAccountList()
        {
            var list = new List<IAccount>();
            for (int i = 0;  i < 10; i++)
            {
                var account = CreateTestAccount();
                list.Add(account);
            }
            return list;
        }
    }
}
