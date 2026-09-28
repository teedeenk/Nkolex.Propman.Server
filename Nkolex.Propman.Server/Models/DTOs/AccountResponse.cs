using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models.DTOs
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool AgreeToTerms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<Guid>? Properties { get; set; }
        public SubscriptionTier SubscriptionTier { get; set; }
        public bool EmailConfirmed { get; set; }

        public static AccountResponse FromAccount(IAccount account)
        {
            ArgumentNullException.ThrowIfNull(account);

            return new AccountResponse
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                PhoneNumber = account.PhoneNumber,
                Email = account.Email,
                AgreeToTerms = account.AgreeToTerms,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt,
                IsDeleted = account.IsDeleted,
                Roles = account.Roles,
                Properties = account.Properties,
                SubscriptionTier = account.SubscriptionTier,
                EmailConfirmed = account.EmailConfirmed
            };
        }
    }
}
