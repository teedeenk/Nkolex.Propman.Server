using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IDataStore
    {
        List<Account> AccountTable { get; set; }
    }
}
