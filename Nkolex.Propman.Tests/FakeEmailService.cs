using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    public class FakeEmailService : IEmailService
    {
        public List<(string Email, string Token)> SentConfirmations { get; } = new();
        public List<(string Email, string Token)> SentPasswordResets { get; } = new();

        public Task SendEmailConfirmationAsync(string email, string token)
        {
            SentConfirmations.Add((email, token));
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string email, string token)
        {
            SentPasswordResets.Add((email, token));
            return Task.CompletedTask;
        }
    }
}
