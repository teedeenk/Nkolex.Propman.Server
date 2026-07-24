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
        public async Task Given_CreateAccountRequest_AddUserAsync_Should_Store_Hashed_Password()
        {
            var createAccountRequest = CreateTestAccountRequest();
            if (_accountService == null)
            {
                throw new InvalidOperationException("AccountService is not registered in the service collection.");
            }

            await _accountService.AddUserAsync(createAccountRequest);

            var accounts = await _accountDataService.GetAllAsync();
            var storedAccount = accounts.First(a => a.Email == createAccountRequest.Email);

            Assert.NotEqual(createAccountRequest.Password, storedAccount.Password);

            var passwordHasher = new Pbkdf2PasswordHasher();
            Assert.True(passwordHasher.VerifyPassword(storedAccount.Password, createAccountRequest.Password));
        }

        [Fact]
        public async Task Given_ValidRole_Should_Be_Able_to_Uprove_Users()
        {
            var account = CreateAccount();
            await _accountDataService.AddAsync(account);
            var sud = await _accountService!.ApproveUser(account);
            Assert.True(sud);
        }

        [Fact]
        public async Task Given_ValidAccount_UpdateUserAsync_Should_Update_User_Info()
        {
            var account = CreateAccount();
            await _accountDataService.AddAsync(account);

            var updateRequest = Factory.Services.GetRequiredService<IAccount>();
            updateRequest.Id = account.Id;
            updateRequest.Name = "Jane";
            updateRequest.Surname = "Smith";
            updateRequest.PhoneNumber = "0987654321";
            updateRequest.Email = "jane.smith@example.com";
            updateRequest.Password = "ShouldNotBeUsed123!";
            updateRequest.AgreeToTerms = account.AgreeToTerms;
            updateRequest.Roles = account.Roles;

            var result = await _accountService!.UpdateUserAsync(updateRequest);
            Assert.True(result);

            var storedAccount = (await _accountDataService.GetAllAsync()).First(a => a.Id == account.Id);
            Assert.Equal("Jane", storedAccount.Name);
            Assert.Equal("Smith", storedAccount.Surname);
            Assert.Equal("0987654321", storedAccount.PhoneNumber);
            Assert.Equal("jane.smith@example.com", storedAccount.Email);
            Assert.Equal(account.Password, storedAccount.Password);
        }

        [Fact]
        public async Task Given_NullAccount_UpdateUserAsync_Should_ThrowException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _accountService!.UpdateUserAsync(null!));
        }

        [Fact]
        public async Task Given_Invalid_Account_UpdateUserAsync_Should_ThrowException()
        {
            var updateRequest = Factory.Services.GetRequiredService<IAccount>();
            updateRequest.Id = Guid.NewGuid();
            updateRequest.Name = "";
            updateRequest.Surname = "Smith";
            updateRequest.PhoneNumber = "0987654321";
            updateRequest.Email = "jane.smith@example.com";

            await Assert.ThrowsAsync<ArgumentException>(() => _accountService!.UpdateUserAsync(updateRequest));
        }

        [Fact]
        public async Task Given_NonExistentAccount_UpdateUserAsync_Should_Return_False()
        {
            var updateRequest = Factory.Services.GetRequiredService<IAccount>();
            updateRequest.Id = Guid.NewGuid();
            updateRequest.Name = "Jane";
            updateRequest.Surname = "Smith";
            updateRequest.PhoneNumber = "0987654321";
            updateRequest.Email = "jane.smith@example.com";

            var result = await _accountService!.UpdateUserAsync(updateRequest);
            Assert.False(result);
        }

        [Fact]
        public async Task Given_NoAccounts_GetAllUsersAsync_Should_Return_EmptyList()
        {
            var result = await _accountService!.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Given_MultipleAccounts_GetAllUsersAsync_Should_Return_All_Accounts()
        {
            var accountOne = CreateAccount();
            await _accountDataService.AddAsync(accountOne);

            var accountTwo = CreateAccount();
            accountTwo.Id = Guid.NewGuid();
            accountTwo.Email = "jane.doe@example.com";
            await _accountDataService.AddAsync(accountTwo);

            var result = await _accountService!.GetAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Id == accountOne.Id);
            Assert.Contains(result, a => a.Id == accountTwo.Id);
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