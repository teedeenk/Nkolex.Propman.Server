using Nkolex.Propman.Server.Abstractions;   

namespace Nkolex.Propman.Server.Models
{
    public class CreateAccountResponse : ICreateAccountResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
