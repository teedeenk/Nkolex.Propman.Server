namespace Nkolex.Propman.Server.Abstractions
{
    public interface IInvoiceService
    {
        Task<IInvoice> AddInvoiceAsync(ICreateInvoiceRequest entity);
    }
}
