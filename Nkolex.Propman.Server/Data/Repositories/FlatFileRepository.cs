using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Models.DTOs;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Nkolex.Propman.Server.Data.Repositories
{
    public class FlatFileRepository : IRepository<IAccount>
    {
        private readonly FlatFileOptions _options;
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<FlatFileRepository> _logger;
        private IDataStore _dataStore;

        public FlatFileRepository(IRepositoryOptions options, IHostEnvironment env, ILogger<FlatFileRepository> logger, IDataStore dataStore)
        {
            _logger = logger;

            _options = options as FlatFileOptions
                ?? throw new ArgumentException("Expected FlatFileOptions");

            if (string.IsNullOrEmpty(_options.FilePath))
                throw new ArgumentException("FilePath must be provided in FlatFileOptions.");

            _filePath = Path.Combine(env.ContentRootPath, _options.FilePath);
            _logger.LogInformation("Flat file path: {FilePath}", _filePath);

            _semaphore = new SemaphoreSlim(1, 1);
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Directory.Exists(directory);
            }

            _dataStore = dataStore;
        }
        public async Task<int> AddAsync(IAccount entity)
        {
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("Entity must have a valid ID before being saved to repository.", nameof(entity));
            }

            await _semaphore.WaitAsync();
            try
            {
                var accounts = await GetAllAccountsFromFileAsync();

                if (accounts.Any(a => a.Id == entity.Id || a.Email == entity.Email))
                {
                    throw new InvalidOperationException($"Account: {entity.Email} already exists.");
                }

                accounts.Add(entity);
                await WriteAccountsToFileAsync(accounts);

                return 1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<int> DeleteAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IAccount>> GetAllAsync()
        {
            try
            { 
                await _semaphore.WaitAsync();
                var accounts = await GetAllAccountsFromFileAsync();
                return accounts;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("There are no accounts {ex}", ex);
                return [];
            }
            finally 
            { 
                _semaphore.Release(); 
            }
        }

        public Task<IAccount> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }

        private async Task<List<IAccount>> GetAllAccountsFromFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }

                var accounts = JsonSerializer.Deserialize<DataStore>(json, _jsonOptions);
                return accounts?.AccountTable.Cast<IAccount>().ToList() ?? [];
            }
            catch (JsonException js)
            {
                _logger.LogError("Error: {js.Message}", js.Message);
                return [];
            }
        }

        private async Task WriteAccountsToFileAsync(List<IAccount> accounts)
        {
            _dataStore.AccountTable = [.. accounts.Cast<Account>()];
            var json = JsonSerializer.Serialize(_dataStore, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
