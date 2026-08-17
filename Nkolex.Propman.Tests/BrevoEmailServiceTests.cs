using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Options;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Nkolex.Propman.Tests
{
    public class BrevoEmailServiceTests
    {
        private readonly IBrevoApiClient _mockBrevoClient;
        private readonly IEmailTemplateProvider _mockTemplateProvider;
        private readonly ILogger<BrevoEmailService> _mockLogger;
        private readonly AppOptions _appOptions;

        public BrevoEmailServiceTests()
        {
            _mockBrevoClient = Substitute.For<IBrevoApiClient>();
            _mockTemplateProvider = Substitute.For<IEmailTemplateProvider>();
            _mockLogger = Substitute.For<ILogger<BrevoEmailService>>();
            _appOptions = new AppOptions { BaseUrl = "https://example.com" };
        }

        private BrevoEmailService CreateService()
        {
            var options = Options.Create(_appOptions);
            return new BrevoEmailService(_mockBrevoClient, _mockTemplateProvider, options, _mockLogger);
        }

        [Fact]
        public async Task Given_ValidEmailAndToken_SendEmailConfirmationAsync_Should_Send_Email()
        {
            var service = CreateService();
            var email = "user@example.com";
            var token = "test-token-123";
            var mockContent = new EmailContent("Confirm Email", "<html>confirm</html>", "confirm");

            _mockTemplateProvider.BuildConfirmationEmail(Arg.Any<string>()).Returns(mockContent);

            await service.SendEmailConfirmationAsync(email, token);

            await _mockBrevoClient.Received(1).SendTransactionalEmailAsync(email, null, mockContent);
        }

        [Fact]
        public async Task Given_ValidEmailAndToken_SendEmailConfirmationAsync_Should_Build_Correct_Confirmation_Link()
        {
            var service = CreateService();
            var email = "user@example.com";
            var token = "test-token-123";
            var mockContent = new EmailContent("Confirm Email", "<html>confirm</html>", "confirm");

            _mockTemplateProvider.BuildConfirmationEmail(Arg.Any<string>()).Returns(mockContent);

            await service.SendEmailConfirmationAsync(email, token);

            var receivedCalls = _mockTemplateProvider.ReceivedCalls();
            Assert.NotEmpty(receivedCalls);

            var linkArg = (string)receivedCalls.First().GetArguments()[0];
            Assert.Contains($"https://example.com/confirm-email?email={Uri.EscapeDataString(email)}", linkArg);
            Assert.Contains($"token={Uri.EscapeDataString(token)}", linkArg);
        }

        [Fact]
        public async Task Given_NullEmail_SendEmailConfirmationAsync_Should_Throw_ArgumentNullException()
        {
            var service = CreateService();
            var token = "test-token-123";

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.SendEmailConfirmationAsync(null!, token));
        }

        [Fact]
        public async Task Given_EmptyEmail_SendEmailConfirmationAsync_Should_Throw_ArgumentNullException()
        {
            var service = CreateService();
            var token = "test-token-123";

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.SendEmailConfirmationAsync("", token));
        }

        [Fact]
        public async Task Given_NullToken_SendEmailConfirmationAsync_Should_Throw_ArgumentNullException()
        {
            var service = CreateService();
            var email = "user@example.com";

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.SendEmailConfirmationAsync(email, null!));
        }

        [Fact]
        public async Task Given_EmptyToken_SendEmailConfirmationAsync_Should_Throw_ArgumentNullException()
        {
            var service = CreateService();
            var email = "user@example.com";

            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.SendEmailConfirmationAsync(email, ""));
        }

        [Fact]
        public async Task Given_ValidEmailAndToken_SendEmailConfirmationAsync_Should_Log_Success()
        {
            var service = CreateService();
            var email = "user@example.com";
            var token = "test-token-123";
            var mockContent = new EmailContent("Confirm Email", "<html>confirm</html>", "confirm");

            _mockTemplateProvider.BuildConfirmationEmail(Arg.Any<string>()).Returns(mockContent);

            await service.SendEmailConfirmationAsync(email, token);

            _mockLogger.Received().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task Given_BrevoClientThrowsException_SendEmailConfirmationAsync_Should_Propagate_Exception()
        {
            var service = CreateService();
            var email = "user@example.com";
            var token = "test-token-123";
            var mockContent = new EmailContent("Confirm Email", "<html>confirm</html>", "confirm");

            _mockTemplateProvider.BuildConfirmationEmail(Arg.Any<string>()).Returns(mockContent);
            _mockBrevoClient.SendTransactionalEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EmailContent>())
                .ThrowsAsync(new InvalidOperationException("Brevo API error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEmailConfirmationAsync(email, token));
        }
    }
}
