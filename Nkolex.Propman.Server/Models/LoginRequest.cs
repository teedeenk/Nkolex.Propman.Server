using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class LoginRequest : IAuthModels
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
