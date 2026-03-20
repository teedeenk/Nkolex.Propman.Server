using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using NuGet.Frameworks;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class AccountServiceTests : TestFixture
    {
        private readonly IAccountService? _accountService;
        private IAccountDataService<IAccount> _accountDataService;


        public AccountServiceTests() : base (new TestWebApplicationFactory<Program>())
        {
            _accountService = Factory.Services.GetRequiredService<IAccountService>();
            _accountDataService = Factory.Services.GetRequiredService<IAccountDataService<IAccount>>();
        }

        [Fact]
        public async Task Given_CreateAccountRequest_AddUserAsync_Should_CreateAccountResponse()
        {
            var createAccountRequest = CreateTestAccountRequest();
            var createAccountResponse = CreateTestAccountResponse();
            if (_accountService == null)
            {
                throw new InvalidOperationException("AccountService is not registered in the service collection.");
            }
            var sud = await _accountService.AddUserAsync(createAccountRequest);
            Assert.NotNull(sud);
            Assert.Equal(createAccountResponse.Message,sud.Message);
        }

        [Fact]
        public async Task Given_Invalid_CreateAccountRequest_AddUserAsync_Should_ThrowException()
        {
            var createAccountRequest = CreateTestAccountRequest();
            createAccountRequest.Email = ""; 
            if (_accountService == null)
            {
                throw new InvalidOperationException("AccountService is not registered in the service collection.");
            }
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accountService.AddUserAsync(createAccountRequest));
        }

        [Fact]
        public async Task Given_ValidRole_Should_Be_Able_to_Uprove_Users()
        {
            var account = CreateAccount();
            await _accountDataService.AddAsync(account);
            var sud = await _accountService!.ApproveUser(account);
            Assert.True(sud);
        }

        private IAccount CreateAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Id = Guid.NewGuid();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = "john.doe@example.com";
            account.Password = "TestPassword123!";
            account.AgreeToTerms = true;
            account.Roles = ["Guest"];
            return account;
        }
        private ICreateAccountRequest CreateTestAccountRequest()
        {
            var createAccountRequest = Factory.Services.GetRequiredService<ICreateAccountRequest>();
            createAccountRequest.Name = "John";
            createAccountRequest.Surname = "Doe";
            createAccountRequest.PhoneNumber = "1234567890";
            createAccountRequest.Email = "john.doe@example.com";
            createAccountRequest.Password = "TestPassword123!";
            createAccountRequest.ConfirmPassword = "TestPassword123!";
            createAccountRequest.AgreeToTerms = true;
            return createAccountRequest;
        }

        private ICreateAccountResponse CreateTestAccountResponse()
        {
            var createAccountResponse = Factory.Services.GetRequiredService<ICreateAccountResponse>();
            createAccountResponse.Success = true;
            createAccountResponse.Message = "Account created successfully";
            createAccountResponse.UserId = "test-user-id";
            return createAccountResponse;
        }

    }
}