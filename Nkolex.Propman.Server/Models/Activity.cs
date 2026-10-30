using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class Activity : IActivity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public bool Draft { get; set; }
        public bool Active { get; set; }
        public bool Done { get; set; }
        public bool IsDeleted { get; set; }
        public Guid PropertyId { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
