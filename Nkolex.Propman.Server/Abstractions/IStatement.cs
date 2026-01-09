using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IStatement
    {
        List<StatementLine> StatementLines { get; set; }
    }
}
