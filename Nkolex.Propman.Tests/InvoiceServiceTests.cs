using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;
using NSubstitute;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]

    public class InvoiceServiceTests : TestFixture
    {
        private readonly IInvoiceService _invoiceService;
        public InvoiceServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _invoiceService = Factory.Services.GetRequiredService<IInvoiceService>();
        }

        [Fact]
        public async Task Given_Valid_Account_AddInvoiceAsync_Should_Create_An_Invoice()
        {
            var request = CreateInvoiceRequest();

            var sud = await _invoiceService.AddInvoiceAsync(request);

            Assert.NotNull(sud);
        }

        [Fact]
        public async Task Given_Valid_Account_AddInvoiceAsync_Should_Return_Invoice_With_InvoiceNumber()
        {
            var request = CreateInvoiceRequest();

            var sud = await _invoiceService.AddInvoiceAsync(request);

            Assert.NotNull(sud.InvoiceNumber);
        }

        [Fact]
        public async Task Given_Valid_Account_And_ATleast_1_InvoiceNum_AddInvoiceAsyn_Should_Return_Next_InvNum()
        {
            var request = CreateInvoiceRequest();
            var invoice = CreateInvoice();
            List<IInvoice> invoices = [invoice];

            var invoiceDataService = Substitute.For<IInvoiceDataService<IInvoice>>();
            invoiceDataService.GetAllAsync().Returns(invoices);

            var invoiceService = new InvoiceService(invoiceDataService);

            var sud = await invoiceService.AddInvoiceAsync(request);

            Assert.NotNull(sud.InvoiceNumber);
        }

        [Fact]
        public async Task Given_Generated_Invoice_isDuplicate_AddInvoiceAsync_Should_Generate_New_Invoice()
        {
            var request = CreateInvoiceRequest();
            var invoice = CreateInvoice();
            List<IInvoice> invoices = [invoice];

            var invoiceDataService = Substitute.For<IInvoiceDataService<IInvoice>>();
            invoiceDataService.GetAllAsync().Returns(invoices);

            var invoiceService = new InvoiceService(invoiceDataService);

            var sud = await invoiceService.AddInvoiceAsync(request);

            Assert.NotEqual(sud,invoice);
        }

        [Theory]
        [InlineData("Inv-000009")]
        public async Task Given_InvoiceNumber_Has_max_digit_AddInvoiceAsync_Should_Generate_New_Invoice_with_theNext_digit(string invNUmber)
        {
            var request = CreateInvoiceRequest();
            var invoice = Factory.Services.GetRequiredService<IInvoice>();
            invoice.InvoiceNumber = invNUmber;
            List<IInvoice> invoices = [invoice];
            var nextInvNumber = "Inv-000010";

            var invoiceDataService = Substitute.For<IInvoiceDataService<IInvoice>>();
            invoiceDataService.GetAllAsync().Returns(invoices);

            var invoiceService = new InvoiceService(invoiceDataService);

            var sud = await invoiceService.AddInvoiceAsync(request);
            Assert.Equal(sud.InvoiceNumber, nextInvNumber);
        }

        private ICreateInvoiceRequest CreateInvoiceRequest()
        {
            var tenant = CreateAccount();
            var propertyAdmin = CreateAccount();

            var request = Factory.Services.GetRequiredService<ICreateInvoiceRequest>();
            request.Tenant = tenant;
            request.PropertyAdmin = propertyAdmin;
            request.Date = DateTime.UtcNow;
            request.DueDate = DateTime.UtcNow.AddDays(30);
            request.InvoiceLines = 
            [
                new InvoiceLine 
                { 
                    Description = "Rent", 
                    Amount = 1000.00m 
                }
            ];
            return request;
        }

        private IInvoice CreateInvoice()
        {
            var invoice = Factory.Services.GetRequiredService<IInvoice>();
            invoice.InvoiceNumber = "Inv-000001";
            return invoice;
        }

        private IAccount CreateAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Id = Guid.NewGuid();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = "john.doe@example.com";
            account.Password = "TestPassword123!";
            account.AgreeToTerms = true;
            account.Roles = ["Guest"];
            return account;
        }
    }
}
