using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Services
{
    public class PasswordHashMigrator
    {
        private readonly IAccountDataService<IAccount> _accountDataService;
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHashMigrator(IAccountDataService<IAccount> accountDataService, IPasswordHasher passwordHasher)
        {
            _accountDataService = accountDataService ?? throw new ArgumentNullException(nameof(accountDataService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task MigrateAsync()
        {
            var accounts = await _accountDataService.GetAllAsync();

            foreach (var account in accounts)
            {
                if (_passwordHasher.IsHashed(account.Password))
                {
                    continue;
                }

                account.Password = _passwordHasher.HashPassword(account.Password);
                await _accountDataService.UpdateAsync(account);
            }
        }
    }
}
