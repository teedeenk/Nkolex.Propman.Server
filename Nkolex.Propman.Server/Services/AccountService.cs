using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Services
{
    public class AccountService : IAccountService
    {
        public Task<ICreateAccountResponse> AddUserAsync(ICreateAccountRequest createAccountRequest)
        {
            if (createAccountRequest == null)
            {
                throw new ArgumentNullException(nameof(createAccountRequest), "CreateAccountRequest cannot be null");
            }

            if (string.IsNullOrWhiteSpace(createAccountRequest.Name) ||
                string.IsNullOrWhiteSpace(createAccountRequest.Surname) ||
                string.IsNullOrWhiteSpace(createAccountRequest.PhoneNumber) ||
                string.IsNullOrWhiteSpace(createAccountRequest.Email) ||
                string.IsNullOrWhiteSpace(createAccountRequest.Password) ||
                createAccountRequest.ConfirmPassword != createAccountRequest.Password ||
                !createAccountRequest.AgreeToTerms)
            {
                throw new ArgumentNullException(nameof(createAccountRequest), "CreateAccountRequest cannot be null");
            }

            ICreateAccountResponse response = new CreateAccountResponse
            {
                Success = true,
                Message = "Account created successfully",
                UserId = "generated-user-id"
            };
            return Task.FromResult(response);
        }
    }
}
