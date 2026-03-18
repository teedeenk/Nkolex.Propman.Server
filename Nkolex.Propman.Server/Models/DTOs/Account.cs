using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models.DTOs
{
    public class Account : IAccount
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public bool AgreeToTerms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> Roles { get; set; } = [ "Guest" ];
        public List<Guid>? Properties { get; set; }
    }
}
