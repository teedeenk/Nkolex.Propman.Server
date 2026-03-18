namespace Nkolex.Propman.Server.Abstractions
{
    public interface IRepository<T>
    {
        Task<int> AddAsync(T entity);
        Task<List<T>> GetAllAsync();
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<T> GetByIdAsync(T entity);
    }
}
