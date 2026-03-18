using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IStatement
    {
        Guid Id { get; set; }
        List<StatementLine> StatementLines { get; set; }
    }
}
