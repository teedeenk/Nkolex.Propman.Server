using System.ComponentModel.DataAnnotations;

namespace Nkolex.Propman.Server.Options
{
    public class BrevoOptions
    {
        public const string SectionName = "Brevo";
        [Required] public string ApiKey { get; set; } = default!;
        [Required, EmailAddress] public string FromEmail { get; set; } = default!;
        public string? FromName { get; set; }
        [Required] public string SendEmailEndpoint { get; set; } = default!;
    }
}
