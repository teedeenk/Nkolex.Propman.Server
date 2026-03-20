using Nkolex.Propman.Server.Abstractions;
using System.Collections.Concurrent;

namespace Nkolex.Propman.Tests
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _store = new();
        private readonly Func<T, Guid> _idSelector;

        public InMemoryRepository(Func<T, Guid> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        public Task<int> AddAsync(T entity)
        {
            var id = _idSelector(entity);
            if (_store.TryAdd(id, entity))
            {
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }

        public Task<List<T>> GetAllAsync()
        {
            return Task.FromResult(_store.Values.ToList());
        }

        public Task<T> GetByIdAsync(T entity)
        {
            var id = _idSelector(entity);
            if (_store.TryGetValue(id, out var result))
            {
                return Task.FromResult(result);
            }
            return Task.FromResult<T>(default!);
        }

        public Task<int> UpdateAsync(T entity)
        {
            var id = _idSelector(entity);
            if (_store.ContainsKey(id))
            {
                _store[id] = entity;
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }

        public Task<int> DeleteAsync(T entity)
        {
            var id = _idSelector(entity);
            if (_store.TryRemove(id, out _))
            {
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }

        public void Clear()
        {
            _store.Clear();
        }
    }
}