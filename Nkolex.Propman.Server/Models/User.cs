using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class User 
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
