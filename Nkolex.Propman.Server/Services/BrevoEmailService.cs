using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<BrevoEmailService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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

            var apiKey = _configuration["Brevo:ApiKey"];
            var fromEmail = _configuration["Brevo:FromEmail"];
            var fromName = _configuration["Brevo:FromName"] ?? fromEmail;
            var baseUrl = _configuration["App:BaseUrl"];
            var sendEmailEndpoint = _configuration["Brevo:SendEmailEndpoint"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(sendEmailEndpoint))
            {
                _logger.LogWarning("Brevo configuration is missing, unable to send email confirmation.");
                throw new InvalidOperationException("Brevo:ApiKey, Brevo:FromEmail, and Brevo:SendEmailEndpoint must be configured.");
            }

            var confirmationLink = $"{baseUrl}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            var payload = new
            {
                sender = new { email = fromEmail, name = fromName },
                to = new[] { new { email } },
                subject = "Confirm your email address",
                textContent = $"Please confirm your email address by clicking the following link: {confirmationLink}"
            };

            var client = _httpClientFactory.CreateClient(nameof(BrevoEmailService));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Remove("api-key");
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            using var response = await client.PostAsJsonAsync(sendEmailEndpoint, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email confirmation via Brevo. Status: {StatusCode}, Body: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Failed to send email via Brevo. Status: {response.StatusCode}");
            }

            _logger.LogInformation("Email confirmation sent to {Email}", email);
        }
    }
}
