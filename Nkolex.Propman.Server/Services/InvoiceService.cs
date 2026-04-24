using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceDataService<IInvoice> _invoiceDataService;
        
        public InvoiceService(IInvoiceDataService<IInvoice> invoiceDataService)
        {
            _invoiceDataService = invoiceDataService;
        }
        
        public async Task<IInvoice> AddInvoiceAsync(ICreateInvoiceRequest entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            var invoices = await _invoiceDataService.GetAllAsync();
            var invoice = GenerateInvoice(invoices);

            if (string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                throw new ArgumentException($"{nameof(invoice.InvoiceNumber)} is empty.");
            }

            invoice.Tenant = entity.Tenant;
            invoice.PropertyAdmin = entity.PropertyAdmin;

            await _invoiceDataService.AddAsync(invoice);

            return invoice;
        }

        private static Invoice GenerateInvoice(List<IInvoice> invoices)
        {
            const string prefix = "Inv-";
            const int defaultInvoiceNumber = 1;
            int nextInvoiceNumber;

            if (invoices is null or [])
            {
                return new Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = $"{prefix}{defaultInvoiceNumber:D6}",
                    Date = DateTime.UtcNow,
                    InvoiceLines = [],
                    PropertyAdmin = new Account(),
                    Tenant = new Account()
                };
            }

            var lastInvoice = invoices.Last();
            var lastInvoiceSuffixString = lastInvoice.InvoiceNumber.Split("-")[1];
            var lastInvoiceNumber = int.Parse(lastInvoiceSuffixString);
            
            nextInvoiceNumber = IsDefaultValue(lastInvoiceNumber) 
                ? defaultInvoiceNumber 
                : lastInvoiceNumber + 1;

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = $"{prefix}{nextInvoiceNumber:D6}",
                Date = DateTime.UtcNow,
                InvoiceLines = [],
                PropertyAdmin = new Account(),
                Tenant = new Account()
            };

            if (IsDuplicate(invoices, invoice))
            {
                invoice.InvoiceNumber = $"{prefix}{(nextInvoiceNumber + 1):D6}";
            }

            return invoice;
        }

        private static bool IsDuplicate(List<IInvoice> invoices, IInvoice invoice)
        {
            return invoices?.Any(item => item.InvoiceNumber == invoice.InvoiceNumber) ?? false;
        }

        private static bool IsDefaultValue(int value)
        {
            return value == default;
        }
    }
}
