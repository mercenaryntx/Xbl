using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.SqlGenerator.Filters;

namespace Xbl.Data.Extensions;

internal static class ReadOnlyDapperRepositoryExtensions
{
    public static SqlQuery GetSelectAllSqlQuery<TEntity>(this IReadOnlyDapperRepository<TEntity> source, Expression expression)
        where TEntity : class
    {
        var visitor = new SqlQueryVisitor(PropertyMappings.GetMapping<TEntity>());
        visitor.Visit(expression);

        var where = (Expression<Func<TEntity, bool>>)visitor.Where;
        var filterData = visitor.FilterData;
        filterData.SelectInfo ??= new SelectInfo {Columns = ["*"]};
        return source.SqlGenerator.GetSelectAll(where, filterData);
    }

    public static SqlQuery GetSelectFirstSqlQuery<TEntity>(this IReadOnlyDapperRepository<TEntity> source, Expression expression)
        where TEntity : class
    {
        var visitor = new SqlQueryVisitor();
        visitor.Visit(expression);

        var where = (Expression<Func<TEntity, bool>>)visitor.Where;
        var filterData = visitor.FilterData;
        filterData.SelectInfo ??= new SelectInfo { Columns = ["*"] };
        return source.SqlGenerator.GetSelectFirst(where, filterData);
    }
}