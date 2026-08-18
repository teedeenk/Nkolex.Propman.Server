namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccountService
    {
        Task<ICreateAccountResponse> AddUserAsync(ICreateAccountRequest createAccountRequest);
        Task<bool> ApproveUser(IAccount account);
        Task<bool> UpdateUserAsync(IAccount account);
        Task<List<IAccount>> GetAllUsersAsync();
        Task<bool> ConfirmEmailAsync(string token);
        Task ResendConfirmationEmailAsync(string email);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
