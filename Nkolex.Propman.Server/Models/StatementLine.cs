using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Models
{
    public record StatementLine
   (
         DateTime Date,
         string Description,
         decimal Amount
    );
}
