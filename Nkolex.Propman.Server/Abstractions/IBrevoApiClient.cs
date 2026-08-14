namespace Nkolex.Propman.Server.Abstractions
{
    public interface IBrevoApiClient
    {
        Task SendTransactionalEmailAsync(string toEmail, string? toName, EmailContent content);
    }
}
