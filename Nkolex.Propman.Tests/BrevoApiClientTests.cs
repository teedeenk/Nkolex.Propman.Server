using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Options;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;

namespace Nkolex.Propman.Tests
{
    public class BrevoApiClientTests
    {
        private readonly ILogger<BrevoApiClient> _mockLogger;
        private readonly BrevoOptions _brevoOptions;

        public BrevoApiClientTests()
        {
            _mockLogger = Substitute.For<ILogger<BrevoApiClient>>();
            _brevoOptions = new BrevoOptions
            {
                ApiKey = "test-api-key-123",
                FromEmail = "sender@example.com",
                FromName = "Propman Support",
                SendEmailEndpoint = "https://api.brevo.com/v3/smtp/email"
            };
        }

        private BrevoApiClient CreateService(HttpClient httpClient)
        {
            var options = Options.Create(_brevoOptions);
            return new BrevoApiClient(httpClient, options, _mockLogger);
        }

        [Fact]
        public async Task Given_SuccessfulResponse_SendTransactionalEmailAsync_Should_Send_Email()
        {
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "recipient@example.com";
            var toName = "Recipient";
            var content = new EmailContent("Test Subject", "<html>Test</html>", "Test");

            await service.SendTransactionalEmailAsync(email, toName, content);

            Assert.True(httpMessageHandler.RequestWasMade);
            Assert.Equal(_brevoOptions.SendEmailEndpoint, httpMessageHandler.RequestUri?.ToString());
        }

        [Fact]
        public async Task Given_FailedResponse_SendTransactionalEmailAsync_Should_Throw_InvalidOperationException()
        {
            var errorResponse = "{\"code\":\"invalid_parameter\",\"message\":\"Invalid email\"}";
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, errorResponse);
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "invalid-email";
            var content = new EmailContent("Test Subject", "<html>Test</html>", "Test");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendTransactionalEmailAsync(email, null, content));

            Assert.Contains("Failed to send email via Brevo", exception.Message);
            Assert.Contains("BadRequest", exception.Message);
        }

        [Fact]
        public async Task Given_FailedResponse_SendTransactionalEmailAsync_Should_Log_Error()
        {
            var errorResponse = "{\"code\":\"quota_exceeded\",\"message\":\"API quota exceeded\"}";
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.TooManyRequests, errorResponse);
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "recipient@example.com";
            var content = new EmailContent("Test Subject", "<html>Test</html>", "Test");

            try
            {
                await service.SendTransactionalEmailAsync(email, null, content);
            }
            catch { }

            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task Given_ValidRequest_SendTransactionalEmailAsync_Should_Set_ApiKeyHeader()
        {
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "recipient@example.com";
            var content = new EmailContent("Test Subject", "<html>Test</html>", "Test");

            await service.SendTransactionalEmailAsync(email, null, content);

            Assert.True(httpMessageHandler.RequestHeaders.Contains("api-key"));
            var apiKeyValues = httpMessageHandler.RequestHeaders.GetValues("api-key");
            Assert.Contains(_brevoOptions.ApiKey, apiKeyValues);
        }

        [Fact]
        public async Task Given_ValidRequest_SendTransactionalEmailAsync_Should_Set_JsonContentType()
        {
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "recipient@example.com";
            var content = new EmailContent("Test Subject", "<html>Test</html>", "Test");

            await service.SendTransactionalEmailAsync(email, null, content);

            Assert.NotNull(httpMessageHandler.ContentType);
            Assert.Contains("application/json", httpMessageHandler.ContentType.ToString());
        }

        [Fact]
        public async Task Given_ValidRequest_SendTransactionalEmailAsync_Should_Include_Correct_Payload()
        {
            var httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
            var httpClient = new HttpClient(httpMessageHandler)
            {
                BaseAddress = new Uri("https://api.brevo.com")
            };

            var service = CreateService(httpClient);
            var email = "recipient@example.com";
            var toName = "John Doe";
            var subject = "Test Subject";
            var htmlContent = "<html>Test</html>";
            var textContent = "Test";
            var content = new EmailContent(subject, htmlContent, textContent);

            await service.SendTransactionalEmailAsync(email, toName, content);

            var requestBody = httpMessageHandler.RequestBody;
            Assert.NotNull(requestBody);
            Assert.Contains(_brevoOptions.FromEmail, requestBody);
            Assert.Contains(email, requestBody);
            Assert.Contains(toName, requestBody);
            Assert.Contains(subject, requestBody);
            Assert.Contains("Test", requestBody);
            Assert.Contains(textContent, requestBody);
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public bool RequestWasMade { get; private set; }
        public Uri? RequestUri { get; private set; }
        public HttpHeaders? RequestHeaders { get; private set; }
        public MediaTypeHeaderValue? ContentType { get; private set; }
        public string? RequestBody { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestWasMade = true;
            RequestUri = request.RequestUri;
            RequestHeaders = request.Headers;
            ContentType = request.Content?.Headers.ContentType;

            if (request.Content != null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
