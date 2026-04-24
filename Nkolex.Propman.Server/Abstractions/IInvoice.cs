using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IInvoice
    {
        Guid Id { get; set; }
        string InvoiceNumber { get; set; }
        IAccount Tenant { get; set; }
        IAccount PropertyAdmin { get; set; }
        DateTime Date { get; set; }
        DateTime DueDate { get; set; }
        List<InvoiceLine> InvoiceLines { get; set; }
    }
}