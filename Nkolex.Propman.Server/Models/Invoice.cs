using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class Invoice : IInvoice
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public required IAccount Tenant { get; set; }
        public required IAccount PropertyAdmin { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public List<InvoiceLine> InvoiceLines { get; set; } = [];
    }
}
