using Microsoft.AspNetCore.Identity;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Services
{
    public class UploadCsvService : IUploadCsvService
    {
        private readonly ILogger<UploadCsvService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUploadCsvDataService<Statement, StatementLine> _uploadCsvDataService;


        public UploadCsvService(ILogger<UploadCsvService> logger, IServiceProvider serviceProvider, IUploadCsvDataService<Statement, StatementLine> uploadCsvDataService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _uploadCsvDataService = uploadCsvDataService;
        }
        public async Task<int> AddAsync(IStatement statement)
        {
            ArgumentNullException.ThrowIfNull(statement);
            try
            {
                var validatedStatement = ValidateStatement(statement);
                if (validatedStatement is not null)
                {
                    if(validatedStatement.StatementLines.Count > 0)
                    {
                        var existingStatement = await _uploadCsvDataService.GetAllAsync();
                        var thereIsADuplicate = IsDuplicate(validatedStatement, existingStatement.FirstOrDefault() ?? new Statement());

                        if (thereIsADuplicate == true)
                        {
                            _logger.LogInformation("A duplicate was found.");
                            return 2;
                        }
                        else
                        {
                            await _uploadCsvDataService.AddAsync(validatedStatement);
                            return 1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError("Error occured while attempting to add Statement, {ex}", ex);
                throw;
            }
        }

        public async Task<List<Statement>> GetAllAsync()
        {
            _logger.LogInformation("Get all statement... | GetAllAsync uploadCsvService");
            var statement = await _uploadCsvDataService.GetAllAsync();
            if(statement is not null)
            {
                _logger.LogInformation("Returning statement {statement} | GetAllAsync uploadCsvService", statement);
                return statement;
            }
            return [];
        }

        private static Statement ValidateStatement(IStatement uploadFile)
        {
            var statement = new Statement();
            foreach (var statementLine in uploadFile.StatementLines)
            {
                if (IsValidDate(statementLine.Date) == true && IsValidDescription(statementLine.Description) == true)
                {
                    statement.StatementLines.Add(statementLine);
                }
            }
            return statement;
        }

        private static bool IsValidDate(DateTime date) => date switch
        {
            { } d when d.ToUniversalTime() == DateTime.MinValue.ToUniversalTime() => false,
            { } d when d > DateTime.UtcNow => false,
            { } d when d.ToUniversalTime() < DateTime.UtcNow && d > DateTime.MinValue.ToUniversalTime() => true,
            _ => false
        };

        private static bool IsValidDescription(string description) => description switch
        {
            { } d when d == string.Empty => false,
            { } d when d == null => false,
            _ => true
        };

        private static bool IsDuplicate(Statement statement, Statement existingStatement)
        {
            bool isDuplicate = false;
            if (existingStatement != null)
            {
                foreach (var statementLine in statement.StatementLines)
                {
                    foreach(var sl in existingStatement.StatementLines)
                    {
                        if (statementLine.Equals(sl))
                        {
                            isDuplicate = true; break;
                        }
                    }
                }
            }
            return isDuplicate;
        }
    }
}
