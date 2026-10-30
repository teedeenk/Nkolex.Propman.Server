namespace Nkolex.Propman.Server.Abstractions
{
    public interface IActivity
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        DateOnly Date { get; set; }
        TimeOnly Time { get; set; }
        bool Draft { get; set; }
        bool Active { get; set; }
        bool Done { get; set; }
        bool IsDeleted { get; set; }
        Guid PropertyId { get; set; }
        Guid CreatedBy { get; set; }
    }
}
