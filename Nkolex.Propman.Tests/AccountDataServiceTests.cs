using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Services;
using Nkolex.Propman.Tests;
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
        private readonly IAccountDataService? _accountDataService;
        public AccountDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _accountDataService = Factory.Services.GetRequiredService<IAccountDataService>();
        }

        [Fact]
        public async Task Given_CreateAccount_AddAsync_Should_Return_1()
        {
            var createAccount = CreateTestAccount();
            if (_accountDataService == null)
            {
                throw new InvalidOperationException("AccountDataService is not registered in the service collection.");
            }
            var sud = await _accountDataService.AddAsync(createAccount);
            Assert.Equal(1,sud);
        }

        private IAccount CreateTestAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = "john.doe@example.com";
            account.Password = "TestPassword123!";
            return account;
        }
    }
}
