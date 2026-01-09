using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IDataStore<T>
    {
        string TableName { get; set; }
        List<T> Data { get; set; }
    }
}
