namespace Xbl.Data;

public interface IDataRepository<T> where T : class
{
    Task Insert(T item, string table = null);
    Task BulkInsert(IEnumerable<T> item, string table = null);
    Task Update(T item, string table = null);
    Task Delete(T item, string table = null);
    Task<T> Get(int id, int partitionKey = 0, string table = null);
    Task<IEnumerable<T>> GetAll(string table = null);
}