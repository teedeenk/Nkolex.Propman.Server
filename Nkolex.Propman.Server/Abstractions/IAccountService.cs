namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccountService
    {
        Task<ICreateAccountResponse> AddUserAsync(ICreateAccountRequest createAccountRequest);
    }
}
