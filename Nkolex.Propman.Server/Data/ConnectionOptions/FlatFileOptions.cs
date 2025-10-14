using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data.ConnectionOptions
{
    public class FlatFileOptions : IRepositoryOptions
    {
        public required string FilePath { get; set; }
    }
}
