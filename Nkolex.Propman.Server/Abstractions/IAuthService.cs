using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAuthService
    {
        Task<User?> ValidateUserAsync(User user, List<User> users);
        Task<List<User>> GetUsersAsync();
        Task<string> GenerateJwtAsync(User user);
    }
}