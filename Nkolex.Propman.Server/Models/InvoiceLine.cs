namespace Nkolex.Propman.Server.Models
{
    public class InvoiceLine
    {
        public DateTime Date {  get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
