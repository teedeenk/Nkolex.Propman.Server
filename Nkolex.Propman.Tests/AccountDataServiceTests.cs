using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using Nkolex.Propman.Tests;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]

    public class AccountDataServiceTests : TestFixture
    {
        private readonly IAccountDataService _accountDataService;
        private IRepository<IAccount> _repo;

        public AccountDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _accountDataService = Factory.Services.GetRequiredService<IAccountDataService>();
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

            var logger = Substitute.For<ILogger<AccountDataService>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService(serviceProvider, logger, _repo);

            var sud = await accountService.GetAllAsync();

            Assert.NotNull(sud);
            Assert.Equal(accounts, sud);
        }

        [Fact]
        public async Task Given_valid_email_GetByIdAsync_Should_Return_Account()
        {
            var account = CreateTestAccount();
            _repo = Substitute.For<IRepository<IAccount>>();
            _repo.GetByIdAsync(account.Email).Returns(new Account { Email = account.Email});

            var logger = Substitute.For<ILogger<AccountDataService>>();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var accountService = new AccountDataService(serviceProvider, logger, _repo);

            var sud = await accountService.GetByIdAsync(account.Email);

            Assert.NotNull(sud);
            Assert.Equal(account.Email, sud.Email);
        }

        private IAccount CreateTestAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Id = Guid.NewGuid();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = "john.doe@example.com";
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
