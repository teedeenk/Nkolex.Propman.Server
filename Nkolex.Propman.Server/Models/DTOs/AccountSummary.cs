namespace Nkolex.Propman.Server.Models.DTOs
{
    public class AccountSummary
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
