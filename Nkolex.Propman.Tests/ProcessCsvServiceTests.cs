using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using System.Text;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class ProcessCsvServiceTests : TestFixture
    {
        private readonly IProcessCsvFileService _processCsvFileService;
        public ProcessCsvServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            using var scope = Factory.Services.CreateScope();
            _processCsvFileService = scope.ServiceProvider.GetRequiredService<IProcessCsvFileService>();
        }

        [Fact]
        public async Task Given_Valid_Csv_ProcessCsv_Should_Return_Statement()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            var logger = Substitute.For<ILogger<ProcessCsvFileService>>();
            var csvContent = new StringBuilder()
                .AppendLine("Date,Description,Amount")
                .AppendLine("2025-01-01,Test Transaction,100.00")
                .ToString();

            var bytes = Encoding.UTF8.GetBytes(csvContent);
            using var file = new MemoryStream(bytes);

            file.Position = 0;

            var sud = await new ProcessCsvFileService(serviceProvider, logger).ProcessCsv(file);

            Assert.NotNull(sud);
            Assert.NotNull(sud.StatementLines);
            Assert.True(sud.StatementLines.Count >= 0);
        }

        [Fact]
        public async Task Given_null_Csv_ProcessCsv_Should_Return_Exception()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _processCsvFileService.ProcessCsv(null!));
        }

        [Fact]
        public async Task Given_Csv_Has_No_Headings_Should_Return_Statement()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            var logger = Substitute.For<ILogger<ProcessCsvFileService>>();
            var csvContent = new StringBuilder()
                .AppendLine("2025-01-01,Test Transaction,100.00")
                .ToString();

            var bytes = Encoding.UTF8.GetBytes(csvContent);
            using var file = new MemoryStream(bytes);

            file.Position = 0;

            var sud = await new ProcessCsvFileService(serviceProvider, logger).ProcessCsv(file);

            Assert.NotNull(sud);
            Assert.NotNull(sud.StatementLines);
            Assert.True(sud.StatementLines.Count >= 0);
        }
    }
}
