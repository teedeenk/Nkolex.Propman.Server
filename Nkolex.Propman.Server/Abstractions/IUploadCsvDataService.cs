
using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IUploadCsvDataService<Statement, StatementLine> : IRepository<Statement> where Statement : class
    {
    }
}