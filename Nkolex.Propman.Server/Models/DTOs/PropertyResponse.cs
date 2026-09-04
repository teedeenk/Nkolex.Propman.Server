namespace Nkolex.Propman.Server.Models.DTOs
{
    public class PropertyResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string PropertyType { get; set; } = string.Empty;
        public Guid Statement { get; set; }
        public AccountSummary? PropertyManager { get; set; }
        public List<AccountSummary> Tenants { get; set; } = [];
    }
}
