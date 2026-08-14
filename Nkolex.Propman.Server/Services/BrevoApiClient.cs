using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Options;
using System.Net.Http.Headers;

namespace Nkolex.Propman.Server.Services
{
    public class BrevoApiClient : IBrevoApiClient
    {
        private readonly HttpClient _client;
        private readonly BrevoOptions _options;
        private readonly ILogger<BrevoApiClient> _logger;

        public BrevoApiClient(HttpClient client, IOptions<BrevoOptions> options, ILogger<BrevoApiClient> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        }

        public async Task SendTransactionalEmailAsync(string toEmail, string? toName, EmailContent content)
        {
            var payload = new
            {
                sender = new { email = _options.FromEmail, name = _options.FromName ?? _options.FromEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject = content.Subject,
                htmlContent = content.HtmlContent,
                textContent = content.TextContent
            };

            using var response = await _client.PostAsJsonAsync(_options.SendEmailEndpoint, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo send failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Failed to send email via Brevo. Status: {response.StatusCode}");
            }
        }
    }
}
