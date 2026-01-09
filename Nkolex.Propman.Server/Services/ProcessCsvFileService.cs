using CsvHelper;
using CsvHelper.Configuration;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.Configs;
using System.Globalization;

namespace Nkolex.Propman.Server.Services
{
    public class ProcessCsvFileService : IProcessCsvFileService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessCsvFileService> _logger;

        public ProcessCsvFileService(IServiceProvider serviceProvider, ILogger<ProcessCsvFileService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task<IStatement> ProcessCsv(Stream csvStream)
        {
            if(csvStream ==  null)
            {
                throw new ArgumentNullException($"Please provide csv {csvStream}");
            }

            try
            {
                var statement = new Statement();
                using var reader = new StreamReader(csvStream);
                var seek = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(seek)) return new Statement();
                bool hasHeader = seek.Any(char.IsLetter);

                csvStream.Position = 0;
                reader.DiscardBufferedData();
                reader.BaseStream.Position = 0;

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",", 
                    HasHeaderRecord = hasHeader,
                    TrimOptions = TrimOptions.Trim,
                    IgnoreBlankLines = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var csv = new CsvReader(reader, config);

                if(!hasHeader)
                {
                    csv.Context.RegisterClassMap<StatementLineMap>();
                }

                var records = csv.GetRecords<StatementLine>().ToList();
                statement.StatementLines.AddRange(records);
                return statement;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error parsing csv file for {ex}", ex.Message);
                throw;
            }
        }
    }
}
