using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Models;
using NSubstitute;
using System;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class UploadCsvDataServiceTests : TestFixture
    {
        private readonly IUploadCsvDataService<Statement, StatementLine> _uploadCsvDataService;
        private IRepository<IStatement> _repo;
        public UploadCsvDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _uploadCsvDataService = Factory.Services.GetRequiredService<IUploadCsvDataService<Statement, StatementLine>>();
            _repo = Factory.Services.GetRequiredService<IRepository<IStatement>>();
        }

        [Fact]
        public async Task Given_No_Statement_AddAsync_Should_Return_ArgumentNullException()
        {
            Statement? uploadFile = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _uploadCsvDataService
            .AddAsync(uploadFile!));
        }

        [Theory]
        [InlineData("0001-01-01T00:00:00Z", "Description", 4.00)]
        [InlineData("2025-01-01T00:00:00Z", "", 4.00)]
        public async Task Given_Invalid_Statement_AddAsync_Should_Return_0(string dateString, string description, decimal amount)
        {
            DateTime date = DateTime.Parse(dateString);
            var uploadFile = new Statement
            {
                StatementLines = [ new(date, description, amount) ]
            };

            var logger = Substitute.For<ILogger<UploadCsvDataService>>();
            _repo = Substitute.For<IRepository<IStatement>>();
            _repo.AddAsync(Arg.Any<IStatement>()).Returns(Task.FromResult(0));

            var result = new UploadCsvDataService(logger, _repo);
            var sud = await result.AddAsync(uploadFile);
            Assert.Equal(0, sud);
        }

        [Theory]
        [InlineData("2025-01-01T00:00:00Z", "Description", 4.00)]
        public async Task Given_valid_Statement_AddAsync_Should_Return_0(string dateString, string description, decimal amount)
        {
            DateTime date = DateTime.Parse(dateString);
            var uploadFile = new Statement
            {
                StatementLines = [new(date, description, amount)]
            };

            var logger = Substitute.For<ILogger<UploadCsvDataService>>();
            _repo = Substitute.For<IRepository<IStatement>>();
            _repo.AddAsync(Arg.Any<Statement>()).Returns(Task.FromResult(1));

            var result = new UploadCsvDataService(logger, _repo);
            var sud = await result.AddAsync(uploadFile);
            Assert.Equal(1, sud);
        }

        [Fact]
        public async Task Given_Valid_Statement_AddAsync_Should_Return_Return_1()
        {
            var uploadFile = CreateUploadFile();
            var sud = await _uploadCsvDataService.AddAsync(uploadFile);
            Assert.Equal(1, sud);
        }

        [Fact]
        public async Task Given_Invalid_Statement_UpdateAsync_Should_Return_Exception()
        {
            Statement? uploadFile = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _uploadCsvDataService
            .UpdateAsync(uploadFile!));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Whole_Statement()
        {
            var statement = CreateUploadFile();
            List<IStatement> ListOfStatement = [statement];

            var logger = Substitute.For<ILogger<UploadCsvDataService>>();
            _repo = Substitute.For<IRepository<IStatement>>();
            _repo.GetAllAsync().Returns(Task.FromResult(ListOfStatement));

            var result = new UploadCsvDataService(logger, _repo);

            var sud = await result.GetAllAsync();
            Assert.NotNull(sud);
            Assert.Collection(sud,
                stmt =>
                {
                    Assert.Equal(3, stmt.StatementLines.Count);
                    Assert.Collection(stmt.StatementLines,
                        line => Assert.Equal("Description one", line.Description),
                        line => Assert.Equal("Description two", line.Description),
                        line => Assert.Equal("Description three", line.Description));
                });
            Assert.Equal(sud, ListOfStatement);
        }

        private static Statement CreateUploadFile() 
        {
            var statement = new Statement();
            statement.StatementLines.Add(new(DateTime.UtcNow, "Description one", 4.01m));
            statement.StatementLines.Add(new(DateTime.UtcNow.AddDays(-1), "Description two", -2.50m));
            statement.StatementLines.Add(new(DateTime.UtcNow.AddDays(1), "Description three", 3.00m));
            return statement;
        }
    }
}
