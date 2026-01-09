using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;
using NSubstitute;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]

    public class UploadCsvServiceTests : TestFixture
    {
        private readonly IUploadCsvService _uploadCsvService;
        private IRepository<IStatement> _repo;
        public UploadCsvServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _uploadCsvService = Factory.Services.GetRequiredService<IUploadCsvService>();
            _repo = Factory.Services.GetRequiredService<IRepository<IStatement>>();
        }

        [Fact]
        public async Task Given_valid_CSV_AddAsync_Should_Return_1()
        {
            var statement = new Statement
            { 
                StatementLines =
                [
                    new(DateTime.UtcNow, "description", 4)
                ]
            };
            var sud = await _uploadCsvService.AddAsync(statement);
            Assert.Equal(1, sud);
        }

        [Fact]
        public async Task Given_Null_CSV_Addsync_should_Return_Exception()
        {
            IStatement statement = null!;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _uploadCsvService.AddAsync(statement));
        }

        [Theory]
        [InlineData("0001-01-01T00:00:00Z", "Description", 4.00)]
        [InlineData("2025-01-01T00:00:00Z", "", 4.00)]
        public async Task Given_Invalid_CSV_AddAsync_Should_Return_0(string dateString, string description, decimal amount)
        {
            DateTime date = DateTime.Parse(dateString);
            var uploadFile = new Statement
            {
                StatementLines = [new(date, description, amount)]
            };

            _repo = Substitute.For<IRepository<IStatement>>();
            _repo.AddAsync(Arg.Any<IStatement>()).Returns(Task.FromResult(0));

            var sud = await _uploadCsvService.AddAsync(uploadFile);
            Assert.Equal(0, sud);
        }

        [Fact]
        public async Task Given_No_Statement_Should_EmptyList()
        {
            var logger = Substitute.For<ILogger<UploadCsvService>>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var listOfStatement = new List<Statement>();
            var dataService = Substitute.For<IUploadCsvDataService<Statement,StatementLine>>();
            dataService.GetAllAsync().Returns(Task.FromResult(listOfStatement));

            var uploadCsvService = new UploadCsvService(logger, serviceProvider, dataService);

            var sud = await uploadCsvService.GetAllAsync();
            Assert.Equal([], sud);
        }
    }
}
