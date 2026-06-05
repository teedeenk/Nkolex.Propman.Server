using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using System;

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
            var propertyService = Substitute.For<IPropertyService>();
            var httpContextaccessor = Substitute.For<IHttpContextAccessor>();

            var uploadCsvService = new UploadCsvService(logger, serviceProvider, dataService, propertyService, httpContextaccessor);

            var sud = await uploadCsvService.GetAllAsync();
            Assert.Equal([], sud);
        }

        [Fact]
        public async Task AddAsync_Should_Update_Property_With_Statement_Id()
        {
            var propertyService = Factory.Services.GetRequiredService<IPropertyService>();
            var logger = Substitute.For<ILogger<UploadCsvService>>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var listOfStatement = new List<Statement>();
            var dataService = Substitute.For<IUploadCsvDataService<Statement, StatementLine>>();
            dataService.GetAllAsync().Returns(Task.FromResult(listOfStatement));
            var property = CreateProperty();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["propertyId"] = property.Id.ToString();

            var httpContextaccessor = Substitute.For<IHttpContextAccessor>();
            httpContextaccessor.HttpContext.Returns(httpContext);

            await propertyService.UploadPropertyAsync(property);
            Assert.NotNull(property);
            
            var statement = new Statement
            {
                StatementLines = [new(DateTime.UtcNow, "Test Description", 100.00m)]
            };

            serviceProvider.GetService<IProperty>().Returns(property);
            var uploadCsvService = new UploadCsvService(logger, serviceProvider, dataService, propertyService, httpContextaccessor);
            var result = await uploadCsvService.AddAsync(statement);
            Assert.Equal(1, result);
            
            var updatedProperty = await propertyService.GetByIdAsync(property);
            Assert.NotEqual(Guid.Empty, updatedProperty.Statement);
            Assert.Equal(statement.Id, updatedProperty.Statement);
        }

        private IProperty CreateProperty()
        {
            var property = Factory.Services.GetRequiredService<IProperty>();
            property.Id = Guid.NewGuid();
            property.Name = "Test Property";
            property.Address = "123 Test St";
            property.PropertyManager = Guid.NewGuid();
            property.Tenants = [];
            property.Statement = Guid.Empty;
            return property;
        }
    }
}
