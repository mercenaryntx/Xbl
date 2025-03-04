using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public interface IRepository<TType> where TType : class, IHaveId
{
    Task Truncate();
    Task Insert(TType item);
    Task BulkInsert(IEnumerable<TType> items);
    Task Update(TType item);
    Task BulkUpdate(IEnumerable<TType> items);
    Task Delete(TType item);
    Task<IEnumerable<TType>> GetAll();

    Task<IEnumerable<IJsonEntity>> GetHeaders();

    Task<TType> Get<TKey>(TKey id, int partitionKey = 0);

    IQueryable<TType> AsQueryable();
}

public interface IRepository<TKey, TType> : IRepository<TType> where TType : class, IHaveId
{
    Task<TType> Get(TKey id, int partitionKey = 0);
}