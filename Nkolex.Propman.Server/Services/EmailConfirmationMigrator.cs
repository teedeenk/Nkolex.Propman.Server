using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Services
{
    public class EmailConfirmationMigrator
    {
        private readonly IAccountDataService<IAccount> _accountDataService;

        public EmailConfirmationMigrator(IAccountDataService<IAccount> accountDataService)
        {
            _accountDataService = accountDataService ?? throw new ArgumentNullException(nameof(accountDataService));
        }

        public async Task MigrateAsync()
        {
            var accounts = await _accountDataService.GetAllAsync();

            foreach (var account in accounts)
            {
                if (account.EmailConfirmed)
                {
                    continue;
                }

                account.EmailConfirmed = true;
                account.EmailConfirmationToken = null;
                account.EmailConfirmationTokenExpiresAt = null;
                await _accountDataService.UpdateAsync(account);
            }
        }
    }
}
