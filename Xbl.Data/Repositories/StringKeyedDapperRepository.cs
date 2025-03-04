using System.Data;
using System.Text.Json;
using Dapper;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public class StringKeyedRepository<T> : DapperRepositoryBase<T>, IRepository<string, T> where T : class, IHaveId
{
    private readonly DapperRepository<T> _innerRepository;

    public StringKeyedRepository(IDbConnection connection, string tableName) : base(connection, tableName)
    {
        _innerRepository = new DapperRepository<T>(connection, new SqlGenerator<T>());
    }

    public async Task<T> Get(string id, int partitionKey = 0)
    {
        var item = await GetWithTypedContainer<StringKeyedJsonEntity>(new { Id = id, PartitionKey = partitionKey });
        return item == null ? null : JsonSerializer.Deserialize<T>(item.Data);
    }

    public Task<T> Get<TKey>(TKey id, int partitionKey = 0)
    {
        if (id is string stringKey) return Get(stringKey, partitionKey);
        throw new InvalidOperationException("Key can only be string here");
    }

    public IQueryable<T> AsQueryable()
    {
        return new DapperQueryable<T>(_innerRepository);
    }

    protected override async Task<IEnumerable<IJsonEntity>> Query(string sql)
    {
        return await Connection.QueryAsync<StringKeyedJsonEntity>(sql);
    }

    protected override IJsonEntity GetContainer(T item) => new StringKeyedJsonEntity((IHaveStringId)item);
}