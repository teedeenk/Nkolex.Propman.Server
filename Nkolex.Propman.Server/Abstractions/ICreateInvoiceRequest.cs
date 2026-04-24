using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface ICreateInvoiceRequest
    {
        IAccount Tenant { get; set; }
        IAccount PropertyAdmin { get; set; }
        DateTime Date { get; set; }
        DateTime DueDate { get; set; }
        List<InvoiceLine> InvoiceLines { get; set; }
    }
}
