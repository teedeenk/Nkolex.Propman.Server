using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class Property : IProperty
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required Guid PropertyManager { get; set; }
        public List<Guid> Tenants { get; set; } = [];
        public Guid Statement { get; set; }
        public string PropertyType { get; set; } = string.Empty;
    }
}
