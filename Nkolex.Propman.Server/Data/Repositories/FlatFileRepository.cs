using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Nkolex.Propman.Server.Data.Repositories
{
    public class FlatFileRepository<T> : IRepository<T>
    {
        private readonly FlatFileOptions _options;
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<FlatFileRepository<T>> _logger;
        private readonly IDataStore<T> _dataStore;
        private readonly ITables _tables;
        private readonly Func<Type, Type?> _concreteTypeResolver;

        public FlatFileRepository(
            IRepositoryOptions options,
            IHostEnvironment env,
            ILogger<FlatFileRepository<T>> logger,
            IDataStore<T> dataStore,
            ITables tables,
            Func<Type, Type?>? concreteTypeResolver = null)
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
            _tables = tables;
            _concreteTypeResolver = concreteTypeResolver ?? FindConcreteImplementationForInterface;
        }

        public async Task<int> AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("Entity must have a valid ID before being saved to repository.", nameof(entity));
            }

            await _semaphore.WaitAsync();
            try
            {
                var tableName = _tables.ResolveNameFor(typeof(T));
                var tIsInterface = typeof(T).IsInterface;

                JsonObject rootObj;
                if (!File.Exists(_filePath))
                {
                    rootObj = [];
                }
                else
                {
                    var fileText = await File.ReadAllTextAsync(_filePath);
                    if (string.IsNullOrWhiteSpace(fileText))
                    {
                        rootObj = [];
                    }
                    else
                    {
                        try
                        {
                            var parsed = JsonNode.Parse(fileText);
                            rootObj = parsed as JsonObject ?? [];
                        }
                        catch (JsonException)
                        {
                            _logger.LogWarning("Flat file JSON invalid; recreating store for table {Table}.", tableName);
                            rootObj = [];
                        }
                    }
                }

                List<T> existingListForTable = [];
                if (rootObj.TryGetPropertyValue(tableName, out var existingNode) && existingNode != null)
                {
                    var arrayText = existingNode.ToJsonString();
                    existingListForTable = DeserializeListFromJsonArray(arrayText, _jsonOptions, tIsInterface, _concreteTypeResolver);
                }

                var merged = false;
                if (existingListForTable.Count > 0)
                {
                    merged = TryMergeCollectionPropertyIntoExisting(existingListForTable, entity);
                }

                if (!merged)
                {
                    existingListForTable.Add(entity);
                }

                _dataStore.TableName = tableName;
                _dataStore.Data = existingListForTable;

                rootObj[tableName] = JsonSerializer.SerializeToNode(existingListForTable, _jsonOptions);

                await File.WriteAllTextAsync(_filePath, rootObj.ToJsonString(options: _jsonOptions));

                return 1;
            }
            catch (JsonException jsEx)
            {
                _logger.LogError(jsEx, "Couldn't add to file.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static List<T> DeserializeListFromJsonArray(string arrayJson, JsonSerializerOptions options, bool typeIsInterface, Func<Type, Type?> findImpl)
        {
            if (string.IsNullOrWhiteSpace(arrayJson)) return new List<T>();

            if (!typeIsInterface)
            {
                try
                {
                    return JsonSerializer.Deserialize<List<T>>(arrayJson, options) ?? new List<T>();
                }
                catch
                {
                    return [];
                }
            }

            var impl = findImpl(typeof(T));
            if (impl == null) return [];

            try
            {
                var listOfImplType = typeof(List<>).MakeGenericType(impl);
                var deserialized = JsonSerializer.Deserialize(arrayJson, listOfImplType, options);
                var result = new List<T>();
                if (deserialized is IEnumerable kv)
                {
                    foreach (var item in kv)
                    {
                        result.Add((T)item);
                    }
                }
                return result;
            }
            catch
            {
                return [];
            }
        }

        public Task<int> DeleteAsync(T entity) => throw new NotImplementedException();

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                var items = await GetAllFromFileAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "There are no records");
                return [];
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<int> UpdateAsync(T entity) => throw new NotImplementedException();

        private async Task<List<T>> GetAllFromFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            try
            {
                var fileText = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(fileText))
                {
                    return [];
                }

                JsonObject rootObj;
                try
                {
                    var parsed = JsonNode.Parse(fileText);
                    rootObj = parsed as JsonObject ?? [];
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Flat file JSON invalid; returning empty list.");
                    return [];
                }

                var tableName = _tables.ResolveNameFor(typeof(T));
                if (!rootObj.TryGetPropertyValue(tableName, out var node) || node == null)
                {
                    return [];
                }

                var arrayText = node.ToJsonString();
                var list = DeserializeListFromJsonArray(arrayText, _jsonOptions, typeof(T).IsInterface, _concreteTypeResolver);
                return list ?? [];
            }
            catch (JsonException js)
            {
                _logger.LogError("Error reading flat file: {js.Message}", js.Message);
                return [];
            }
        }

        private static Type? FindConcreteImplementationForInterface(Type interfaceType)
        {
            try
            {
                var candidates = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
                    .ToArray();

                return candidates.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static bool TryMergeCollectionPropertyIntoExisting(List<T> existingList, T incoming)
        {
            if (incoming == null) return false;
            if (existingList == null) return false;
            if (existingList.Count == 0) return false; 

            var collectionProp = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p =>
                    p.CanRead && p.CanWrite &&
                    p.PropertyType != typeof(string) &&
                    p.PropertyType.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

            if (collectionProp == null) return false;

            var existingTarget = existingList[0];
            var existingCollectionObj = collectionProp.GetValue(existingTarget);
            if (collectionProp.GetValue(incoming) is not IEnumerable incomingCollectionObj) return false;

            var enumInterface = collectionProp.PropertyType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumInterface == null) return false;
            var itemType = enumInterface.GetGenericArguments()[0];

            if (existingCollectionObj == null)
            {
                var listType = typeof(List<>).MakeGenericType(itemType);
                var newList = (IList)Activator.CreateInstance(listType)!;
                foreach (var it in incomingCollectionObj)
                {
                    newList.Add(it);
                }
                collectionProp.SetValue(existingTarget, newList);
                return true;
            }

            if (existingCollectionObj is IList existingIList)
            {
                foreach (var it in incomingCollectionObj)
                {
                    existingIList.Add(it);
                }
                return true;
            }

            var newListType = typeof(List<>).MakeGenericType(itemType);
            var newListInstance = (IList)Activator.CreateInstance(newListType)!;

            foreach (var ex in (existingCollectionObj as IEnumerable) ?? Enumerable.Empty<object>())
            {
                newListInstance.Add(ex);
            }

            foreach (var inc in incomingCollectionObj)
            {
                newListInstance.Add(inc);
            }

            collectionProp.SetValue(existingTarget, newListInstance);
            return true;
        }
    }
}
