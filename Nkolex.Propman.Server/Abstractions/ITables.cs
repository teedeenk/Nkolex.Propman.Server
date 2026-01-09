namespace Nkolex.Propman.Server.Abstractions
{
    public interface ITables
    {
        List<string> TableNames { get; set; }
        string ResolveNameFor(Type entityType);
    }
}
