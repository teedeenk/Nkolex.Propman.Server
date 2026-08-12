using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Nkolex.Propman.Server.Abstractions;

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

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(from));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Confirm your email address";
            message.Body = new TextPart("plain")
            {
                Text = $"Please confirm your email address by clicking the following link: {confirmationLink}"
            };

            using var client = new SmtpClient();
            var secureSocketOptions = port == 465
                ? SecureSocketOptions.SslOnConnect
                : enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

            await client.ConnectAsync(host, port, secureSocketOptions);
            if (!string.IsNullOrWhiteSpace(user))
            {
                await client.AuthenticateAsync(user, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email confirmation sent to {Email}", email);
        }
    }
}
