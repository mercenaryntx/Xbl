using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public interface IRepository
{
    Task Truncate();
    Task<IEnumerable<IJsonEntity>> GetHeaders();
}

public interface IRepository<TType> : IRepository where TType : class, IHaveId
{
    Task Insert(TType item);
    Task BulkInsert(IEnumerable<TType> items);
    Task Update(TType item);
    Task BulkUpdate(IEnumerable<TType> items);
    Task Delete(TType item);
    Task<TType> Get<TKey>(TKey id, int partitionKey = 0);
    Task<IEnumerable<TType>> GetAll();
    IQueryable<TType> AsQueryable();
}

public interface IRepository<TKey, TType> : IRepository<TType> where TType : class, IHaveId
{
    Task<TType> Get(TKey id, int partitionKey = 0);
}