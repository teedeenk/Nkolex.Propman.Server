namespace Nkolex.Propman.Server.Abstractions
{
    public interface IAccountDataService<IAccount> : IRepository<IAccount> where IAccount : class
    {

    }
}
