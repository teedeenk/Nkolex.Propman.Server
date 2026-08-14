using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Nkolex.Propman.Server.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IBrevoApiClient _brevoClient;
        private readonly IEmailTemplateProvider _templates;
        private readonly AppOptions _appOptions;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(
            IBrevoApiClient brevoClient,
            IEmailTemplateProvider templates,
            IOptions<AppOptions> appOptions,
            ILogger<BrevoEmailService> logger)
        {
            _brevoClient = brevoClient;
            _templates = templates;
            _appOptions = appOptions.Value;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));

            var confirmationLink =
                $"{_appOptions.BaseUrl}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            var content = _templates.BuildConfirmationEmail(confirmationLink);

            await _brevoClient.SendTransactionalEmailAsync(email, toName: null, content);

            _logger.LogInformation("Email confirmation sent to {Email}", email);
        }
    }
}
