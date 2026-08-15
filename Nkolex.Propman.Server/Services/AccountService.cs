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
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IServiceProvider serviceProvider, IPasswordHasher passwordHasher, IEmailService emailService, ILogger<AccountService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger;
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
            int added;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();
                added = await dataService.AddAsync(entity);
            }

            if (added == 0)
            {
                return new CreateAccountResponse
                {
                    Success = false,
                    Message = "An account with this email already exists",
                    UserId = string.Empty
                };
            }

            try
            {
                await _emailService.SendEmailConfirmationAsync(entity.Email, entity.EmailConfirmationToken!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email confirmation to {Email}", entity.Email);
            }

            ICreateAccountResponse response = new CreateAccountResponse
            {
                Success = true,
                Message = "Account created successfully",
                UserId = "generated-user-id"
            };
            return response;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentNullException(nameof(token));
                }

                using var scope = _serviceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IAccountDataService<IAccount>>();

                var account = await dataService.GetByEmailConfirmationTokenAsync(token);
                if (account == null)
                {
                    return false;
                }

                if (account.EmailConfirmed)
                {
                    return true;
                }

                if (account.EmailConfirmationTokenExpiresAt == null ||
                    account.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
                {
                    return false;
                }

                account.EmailConfirmed = true;
                account.EmailConfirmationToken = null;
                account.EmailConfirmationTokenExpiresAt = null;
                account.UpdatedAt = DateTime.UtcNow;

                var update = await dataService.UpdateAsync(account);
                return update != 0;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email with token {Token}", token);
                return false;
            }
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
                DeletedAt = null,
                EmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString("N"),
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            return account;
        }
    }
}
