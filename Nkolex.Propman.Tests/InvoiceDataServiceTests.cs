using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using NSubstitute;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class InvoiceDataServiceTests : TestFixture
    {
        private readonly IInvoiceDataService<IInvoice> _invoiceService;
        private IRepository<IInvoice> _repo;
        public InvoiceDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _invoiceService = Factory.Services.GetRequiredService<IInvoiceDataService<IInvoice>>();
            _repo = Factory.Services.GetRequiredService<IRepository<IInvoice>>();
        }

        [Fact]
        public async Task Given_valid_CSV_AddAsync_Should_Return_1()
        {
            var statement = new Invoice
            {
                InvoiceNumber = "Inv-0000001",
                Tenant = Substitute.For<IAccount>(),
                PropertyAdmin = Substitute.For<IAccount>(),
                Date = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                InvoiceLines = []
            };
            var sud = await _invoiceService.AddAsync(statement);
            Assert.Equal(1, sud);
        }

        [Fact]
        public async Task Given_Null_CSV_Addsync_should_Return_Exception()
        {
            IInvoice invoice = null!;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _invoiceService.AddAsync(invoice));
        }
    }
}
