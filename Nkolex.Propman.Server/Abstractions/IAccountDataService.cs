namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccountDataService<IAccount> : IRepository<IAccount> where IAccount : class
    {
        Task<IAccount> GetByEmailConfirmationTokenAsync(string token);
        Task<IAccount> GetByPasswordResetTokenAsync(string token);
    }
}
