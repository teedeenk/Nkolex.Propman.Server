using Nkolex.Propman.Server.Abstractions;
using System.Net;
using System.Net.Mail;

namespace Nkolex.Propman.Server.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var host = _configuration["Smtp:Host"];
            var port = _configuration.GetValue<int>("Smtp:Port");
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"];
            var enableSsl = _configuration.GetValue("Smtp:EnableSsl", true);
            var baseUrl = _configuration["App:BaseUrl"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("Smtp configuration is missing, unable to send email confirmation.");
                throw new InvalidOperationException("Smtp:Host and Smtp:From must be configured.");
            }

            var confirmationLink = $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            using var message = new MailMessage(from, email)
            {
                Subject = "Confirm your email address",
                Body = $"Please confirm your email address by clicking the following link: {confirmationLink}",
                IsBodyHtml = false
            };

            using var client = new SmtpClient(host, port);
            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(user, password);
            }
            client.EnableSsl = enableSsl;

            await client.SendMailAsync(message);
            _logger.LogInformation("Email confirmation sent to {Email}", email);
        }
    }
}
