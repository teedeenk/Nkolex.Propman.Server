namespace Nkolex.Propman.Server.Abstractions
{
    public interface IProperty
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        string PropertyManager { get; set; }
        List<Guid> Tenants { get; set; }
        Guid Statement { get; set; }
    }
}
