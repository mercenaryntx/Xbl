using Microsoft.Data.Sqlite;
using Xbl.Data.Entities;
using Xbl.Data.Repositories;

namespace Xbl.Data;

public interface IDatabaseContext
{
    bool IsExists { get; }
    bool IsVoid { get; } 
    bool IsReadOnly { get; }

    Task<IRepository<T>> GetRepository<T>(string tableName = null) where T : class, IHaveId;
    Task<IEnumerable<T>> Query<T>(string query, object param = null);
    Task<T> QuerySingle<T>(string query, object param = null);
    IQueryable<T> AsQueryable<T>(string tableName = null) where T : class, IHaveId;

    Task EnsureTable(string tableName);
    IDatabaseContext Mandatory(SqliteOpenMode mode = SqliteOpenMode.ReadOnly);
    IDatabaseContext Optional();
}