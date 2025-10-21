using Microsoft.Extensions.Options;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Models.DTOs;
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

        public FlatFileRepository(IRepositoryOptions options, IHostEnvironment env, ILogger<FlatFileRepository> logger)
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

                if (accounts.Any(a => a.Id == entity.Id))
                {
                    throw new InvalidOperationException($"An account with ID {entity.Id} already exists.");
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
                return new List<IAccount>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<IAccount>();
                }

                var accounts = JsonSerializer.Deserialize<List<Account>>(json, _jsonOptions);
                return accounts?.Cast<IAccount>().ToList() ?? new List<IAccount>();
            }
            catch (JsonException)
            {
                return new List<IAccount>();
            }
        }

        private async Task WriteAccountsToFileAsync(List<IAccount> accounts)
        {
            var concreteAccounts = accounts.Select(a => new Account
            {
                Id = a.Id,
                Name = a.Name,
                Surname = a.Surname,
                Email = a.Email,
                Password = a.Password,
                AgreeToTerms = a.AgreeToTerms,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                DeletedAt = a.DeletedAt,
                IsDeleted = a.IsDeleted,
                PhoneNumber = a.PhoneNumber
            }).ToList();

            var json = JsonSerializer.Serialize(concreteAccounts, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }

}
