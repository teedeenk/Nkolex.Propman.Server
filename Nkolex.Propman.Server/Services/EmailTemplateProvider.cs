using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Services
{
    public class EmailTemplateProvider : IEmailTemplateProvider
    {
        private readonly EmbeddedTemplateLoader _loader;
        private readonly IConfiguration _configuration;

        public EmailTemplateProvider(IConfiguration configuration)
        {
            _loader = new EmbeddedTemplateLoader("Nkolex.Propman.Server.EmailTemplates");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public EmailContent BuildConfirmationEmail(string confirmationLink)
        {
            var layout = _loader.Load("Layout.html");
            var body = _loader.Load("ConfirmEmail.html");

            body = TemplateRenderer.Render(body, new Dictionary<string, string>
            {
                ["ConfirmationLink"] = confirmationLink,
                ["ExpiryHours"] = "24"
            });

            var html = TemplateRenderer.Render(layout, new Dictionary<string, string>
            {
                ["BODY"] = body,
                ["AppName"] = _configuration["Email:AppName"] ?? "",
                ["LogoUrl"] = _configuration["Email:LogoUrl"] ?? "",
                ["Year"] = DateTime.UtcNow.Year.ToString()
            });

            var text = $"Please confirm your email address by visiting: {confirmationLink}";

            return new EmailContent("Confirm your email address", html, text);
        }

        public EmailContent BuildPasswordResetEmail(string resetLink)
        {
            var layout = _loader.Load("Layout.html");
            var body = _loader.Load("ResetPassword.html");

            body = TemplateRenderer.Render(body, new Dictionary<string, string>
            {
                ["ResetLink"] = resetLink,
                ["ExpiryHours"] = "24"
            });

            var html = TemplateRenderer.Render(layout, new Dictionary<string, string>
            {
                ["BODY"] = body,
                ["AppName"] = _configuration["Email:AppName"] ?? "",
                ["LogoUrl"] = _configuration["Email:LogoUrl"] ?? "",
                ["Year"] = DateTime.UtcNow.Year.ToString()
            });

            var text = $"Reset your password by visiting: {resetLink}";

            return new EmailContent("Reset your password", html, text);
        }
    }
}
