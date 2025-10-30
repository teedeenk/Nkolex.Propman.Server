using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class LoginResponse : IAuthModels
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
