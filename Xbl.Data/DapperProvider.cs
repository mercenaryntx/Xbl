using System.Collections;
using System.Linq.Expressions;
using System.Text.Json;
using Dapper;
using MicroOrm.Dapper.Repositories;
using Microsoft.Data.Sqlite;
using Xbl.Data.Entities;
using Xbl.Data.Extensions;

namespace Xbl.Data;

public class DapperProvider<TEntity, TProjection> : IQueryProvider where TEntity : class
{
    private readonly IReadOnlyDapperRepository<TEntity> _inner;
    public Expression Expression { get; }

    public DapperProvider(IReadOnlyDapperRepository<TEntity> inner, Expression expression)
    {
        _inner = inner;
        Expression = expression;
    }

    public IQueryable CreateQuery(Expression expression) => CreateQuery<object>(expression);

    public IQueryable<TNewTarget> CreateQuery<TNewTarget>(Expression expression)
    {
        return new DapperQueryable<TEntity, TNewTarget>(_inner, expression);
    }

    public object Execute(Expression expression) => Execute<object>(expression);

    public T Execute<T>(Expression expression) =>
        typeof(IEnumerable).IsAssignableFrom(expression.Type)
            ? (T)ExecuteAsEnumerable(expression)
            : (T)ExecuteAsSingle(expression);

    private object ExecuteAsEnumerable(Expression expression)
    {
        var visitor = _inner.Visit(expression);
        var where = (Expression<Func<TEntity, bool>>)visitor.Where;
        var query = _inner.SqlGenerator.GetSelectAll(where, visitor.FilterData);
        var sql = query.GetSql().FixFieldReferencesInQuery<TEntity>();

        return !visitor.UsedPropertyMapping
            ? MapToEntity(sql, query.Param)
            : _inner.Connection.Query(sql, query.Param).Map<TProjection>();
    }

    private object ExecuteAsSingle(Expression expression)
    {
        var visitor = _inner.Visit(expression);
        var where = (Expression<Func<TEntity, bool>>)visitor.Where;
        var query = _inner.SqlGenerator.GetSelectFirst(where, visitor.FilterData);
        var sql = query.GetSql().FixFieldReferencesInQuery<TEntity>();

        return visitor.MethodName switch
        {
            nameof(Queryable.Count) => _inner.Connection.QueryFirst<int>(sql, query.Param),
            nameof(Queryable.Any) => _inner.Connection.QueryFirst<int>(sql, query.Param) > 0,
            _ => MapToEntity(sql, query.Param).FirstOrDefault()
        };
    }

    private IEnumerable<TEntity> MapToEntity(string sql, object param)
    {
        var interfaces = typeof(TEntity).GetInterfaces();
        if (interfaces.Contains(typeof(IHaveIntId))) return ParseResult<IntKeyedJsonEntity>(sql, param);
        if (interfaces.Contains(typeof(IHaveStringId))) return ParseResult<StringKeyedJsonEntity>(sql, param);

        throw new InvalidOperationException("Type must implement either `IHaveIntId` or `IHaveStringId`");

    }

    private IEnumerable<TEntity> ParseResult<TContainer>(string sql, object param) where TContainer : IJsonEntity
    {
        try
        {
            var items = _inner.Connection.Query<TContainer>(sql, param);
            return items.Select(i => JsonSerializer.Deserialize<TEntity>(i.Data));
        }
        catch (SqliteException e)
        {
            throw new Exception($"Query execution failed: {sql}", e);
        }
    }
}