using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    public class FakeEmailService : IEmailService
    {
        public List<(string Email, string Token)> SentConfirmations { get; } = new();

        public Task SendEmailConfirmationAsync(string email, string token)
        {
            SentConfirmations.Add((email, token));
            return Task.CompletedTask;
        }
    }
}
