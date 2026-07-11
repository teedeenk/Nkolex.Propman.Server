using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using System.Reflection.PortableExecutable;

namespace Nkolex.Propman.Server.Services
{
    public class AccountService : IAccountService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPasswordHasher _passwordHasher;

        public AccountService(IServiceProvider serviceProvider, IPasswordHasher passwordHasher)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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

            IAccount entity = RequestToAccount(createAccountRequest, _passwordHasher);
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

        public async Task<bool> ApproveUser(IAccount account)
        {
            ArgumentNullException.ThrowIfNull(account);

            if (account.Roles == null || account.Roles.Count != 0 || account.Roles.FirstOrDefault() == "Guest")
            {
                using var scope = _serviceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();
                var updatedAccount = UpdateAccount(account);
                var update = await dataService.UpdateAsync(updatedAccount);

                if(update == 0)
                {
                    return false;
                }
            }

            return true;
        }
        private static IAccount UpdateAccount(IAccount account)
        {
            account.Roles.Add("Tenant");
            return account;
        }
        private static IAccount RequestToAccount(ICreateAccountRequest createAccountRequest, IPasswordHasher passwordHasher)
        {
            IAccount account = new Account
            {
                Id = Guid.NewGuid(),
                Name = createAccountRequest.Name,
                Surname = createAccountRequest.Surname,
                PhoneNumber = createAccountRequest.PhoneNumber,
                Email = createAccountRequest.Email,
                Password = passwordHasher.HashPassword(createAccountRequest.Password),
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
