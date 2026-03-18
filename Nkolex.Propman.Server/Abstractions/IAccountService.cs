namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccountService
    {
        Task<ICreateAccountResponse> AddUserAsync(ICreateAccountRequest createAccountRequest);
        Task<bool> ApproveUser(IAccount account);
    }
}
