namespace Nkolex.Propman.Server.Abstractions
{
    public interface IProperty
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        Guid PropertyManager { get; set; }
        string PropertyType { get; set; }
        List<Guid> Tenants { get; set; }
        Guid Statement { get; set; }
    }
}
