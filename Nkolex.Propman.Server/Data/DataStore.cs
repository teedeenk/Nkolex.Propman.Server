using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Data
{
    public class DataStore : IDataStore
    {
        public List<Account> AccountTable { get; set; } = [];
    }
}
