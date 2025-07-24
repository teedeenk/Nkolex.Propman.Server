namespace Nkolex.Propman.Server.Abstractions
{
    public interface IRepository<T>
    {
        Task<int> AddAsync(T entity);

    }
}
