using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class AccountServiceTests : TestFixture
    {
        private readonly IAccountService? _accountService;

        public AccountServiceTests() : base (new TestWebApplicationFactory<Program>())
        {
            _accountService = _factory.Services.GetRequiredService<IAccountService>();
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
        private ICreateAccountRequest CreateTestAccountRequest()
        {
            var createAccountRequest = _factory.Services.GetRequiredService<ICreateAccountRequest>();
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
            var createAccountResponse = _factory.Services.GetRequiredService<ICreateAccountResponse>();
            createAccountResponse.Success = true;
            createAccountResponse.Message = "Account created successfully";
            createAccountResponse.UserId = "test-user-id";
            return createAccountResponse;
        }

    }
}