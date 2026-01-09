using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using System.Reflection.PortableExecutable;

namespace Nkolex.Propman.Server.Services
{
    public class AccountService : IAccountService
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<ICreateAccountResponse> AddUserAsync(ICreateAccountRequest createAccountRequest)
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

            IAccount entity = RequestToAccount(createAccountRequest);
            using (var scope = _serviceProvider.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();
                await dataService.AddAsync(entity);
            }

            ICreateAccountResponse response = new CreateAccountResponse
            {
                Success = true,
                Message = "Account created successfully",
                UserId = "generated-user-id"
            };
            return response;
        }
        private static IAccount RequestToAccount(ICreateAccountRequest createAccountRequest)
        {
            IAccount account = new Account
            {
                Id = Guid.NewGuid(),
                Name = createAccountRequest.Name,
                Surname = createAccountRequest.Surname,
                PhoneNumber = createAccountRequest.PhoneNumber,
                Email = createAccountRequest.Email,
                Password = createAccountRequest.Password,
                AgreeToTerms = createAccountRequest.AgreeToTerms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null
            };
            return account;
        }
    }
}
