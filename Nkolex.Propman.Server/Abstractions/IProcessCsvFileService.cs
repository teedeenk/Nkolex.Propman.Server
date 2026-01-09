namespace Nkolex.Propman.Server.Abstractions
{
    public interface IProcessCsvFileService
    {
        Task<IStatement> ProcessCsv(Stream csvStream);
    }
}
