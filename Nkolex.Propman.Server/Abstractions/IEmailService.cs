namespace Nkolex.Propman.Server.Abstractions
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string token);
    }
}
