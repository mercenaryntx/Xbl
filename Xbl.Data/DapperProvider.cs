using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Dapper;
using MicroOrm.Dapper.Repositories;
using Microsoft.Data.Sqlite;
using Xbl.Data.Entities;
using Xbl.Data.Extensions;

namespace Xbl.Data;

public class DapperProvider<TEntity> : IQueryProvider where TEntity : class
{
    private readonly IReadOnlyDapperRepository<TEntity> _inner;
    public Expression Expression { get; }

    public DapperProvider(IReadOnlyDapperRepository<TEntity> inner, Expression expression)
    {
        _inner = inner;
        Expression = expression;
    }

    public IQueryable CreateQuery(Expression expression) => CreateQuery<object>(expression);

    public IQueryable<TNewOutput> CreateQuery<TNewOutput>(Expression expression)
    {
        if (typeof(TNewOutput) != typeof(TEntity))
        {
            throw new NotSupportedException("Projection is not supported");
        }
        return (IQueryable<TNewOutput>)new DapperQueryable<TEntity>(_inner, expression);
    }

    public object Execute(Expression expression) => Execute<object>(expression);

    public T Execute<T>(Expression expression)
    {
        return (T)ExecuteDefault(expression, typeof(T));
    }

    private object ExecuteDefault(Expression expression, Type resultType)
    {
        return typeof(IEnumerable).IsAssignableFrom(expression.Type)
            ? ExecuteAsEnumerable(expression)
            : ExecuteAsSingle(expression, resultType);
    }

    private object ExecuteAsEnumerable(Expression expression)
    {
        var query = _inner.GetSelectAllSqlQuery(expression);
        var sql = ReplaceFieldReferences(query.GetSql());

        var methodExpression = expression as MethodCallExpression;
        if (methodExpression?.Method.Name != "DistinctBy")
        {
            return TypeSwitch(sql, query.Param);
        }

        var result = _inner.Connection.Query(sql, query.Param);
        var mapper = new MapperConfiguration(c =>
        {
            c.CreateMap<IDictionary<string, object>, TEntity>()
                .ConvertUsing((source, destination, context) =>
                {
                    destination ??= Activator.CreateInstance<TEntity>();

                    foreach (var (key, value) in source)
                    {
                        var property = destination.GetType().GetProperty(key);
                        if (property != null && property.CanWrite)
                        {
                            var mapped = context.Mapper.Map(value, value.GetType(), property.PropertyType);
                            property.SetValue(destination, mapped);
                        }
                    }
                    return destination;
                });

        }).CreateMapper();
        var x = result.Select(r => mapper.Map<TEntity>((IDictionary<string, object>)r));
        return x;
    }

    private object ExecuteAsSingle(Expression expression, Type resultType)
    {
        object result;
        var query = _inner.GetSelectFirstSqlQuery(expression);
        var sql = ReplaceFieldReferences(query.GetSql());
        var methodExpression = expression as MethodCallExpression;
        if (methodExpression?.Method.Name == "Count")
        {
            sql = Regex.Replace(sql, "SELECT (.*?) FROM", "SELECT COUNT(*) FROM");
            result = _inner.Connection.QueryFirst<int>(sql, query.Param);
        }
        else
        {
            result = TypeSwitch(sql, query.Param).FirstOrDefault();
        }

        if (typeof(bool).IsAssignableFrom(resultType))
        {
            result = result != null;
        }
        return result;
    }

    private IEnumerable<TEntity> TypeSwitch(string sql, object param)
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

    private string ReplaceFieldReferences(string query)
    {
        const string where = @"(?<=FROM\s+(?<table>\w+)\s+WHERE\s+)(?<condition>(\k<table>\.\w+\s*=\s*@\w+)(?:\s+AND\s+|\s+OR\s+)?)+";
        const string orderBy = @"(?<=ORDER BY )(?:(?:(?<column>\w+)(?:\sASC|\sDESC|)\s*)(?:\s*,\s*)?)+";

        var mapping = PropertyMappings.GetMapping<TEntity>();

        query = Regex.Replace(query, where, match =>
        {
            var table = match.Groups["table"].Value;
            var condition = match.Groups["condition"].Value;
            return Regex.Replace(condition, $@"{table}\.(\w+)\s*=\s*(@\w+)", mm =>
            {
                var field = mm.Groups[1].Value;
                return $"{mapping[field].ColumnName} = {mm.Groups[2].Value}";
            });
        });

        return Regex.Replace(query, orderBy, match =>
        {
            var columns = match.Groups["column"].Captures;
            return string.Join(", ", columns.Select(c => mapping[c.Value].ColumnName));
        });
    }
}