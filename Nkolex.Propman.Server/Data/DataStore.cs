using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Data
{
    public class DataStore<T> : IDataStore<T>
    {
        public string TableName { get; set; } = string.Empty;
        public List<T> Data { get; set; } = [];
    }
}
