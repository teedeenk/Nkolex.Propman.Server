namespace Nkolex.Propman.Server.Abstractions
{
    public interface IRepository<T>
    {
        Task<int> AddAsync(T entity);
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(string email);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
    }
}
