using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public class Statement : IStatement
    {
        public List<StatementLine> StatementLines { get; set; } = [];
    }
}
