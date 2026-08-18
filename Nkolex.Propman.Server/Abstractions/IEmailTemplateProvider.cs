namespace Nkolex.Propman.Server.Abstractions
{
    public record EmailContent(string Subject, string HtmlContent, string TextContent);

    public interface IEmailTemplateProvider
    {
        EmailContent BuildConfirmationEmail(string confirmationLink);
        EmailContent BuildPasswordResetEmail(string resetLink);
    }
}
