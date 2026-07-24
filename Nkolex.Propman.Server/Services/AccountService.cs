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
        public async Task<bool> UpdateUserAsync(IAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account), "Account cannot be null");
            }

            if (account.Id == Guid.Empty ||
                string.IsNullOrWhiteSpace(account.Name) ||
                string.IsNullOrWhiteSpace(account.Surname) ||
                string.IsNullOrWhiteSpace(account.PhoneNumber) ||
                string.IsNullOrWhiteSpace(account.Email))
            {
                throw new ArgumentException("Account must have a valid Id, Name, Surname, PhoneNumber and Email", nameof(account));
            }

            using var scope = _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();

            var accounts = await dataService.GetAllAsync();
            var existingAccount = accounts.FirstOrDefault(a => a.Id == account.Id);
            if (existingAccount == null)
            {
                return false;
            }

            existingAccount.Name = account.Name;
            existingAccount.Surname = account.Surname;
            existingAccount.PhoneNumber = account.PhoneNumber;
            existingAccount.Email = account.Email;
            existingAccount.AgreeToTerms = account.AgreeToTerms;
            existingAccount.Roles = account.Roles;
            existingAccount.Properties = account.Properties;
            existingAccount.SubscriptionTier = account.SubscriptionTier;
            existingAccount.UpdatedAt = DateTime.UtcNow;

            var update = await dataService.UpdateAsync(existingAccount);
            return update != 0;
        }

        public async Task<List<IAccount>> GetAllUsersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();
            return await dataService.GetAllAsync();
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
