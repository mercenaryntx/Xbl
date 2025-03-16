using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories;
using MicroOrm.Dapper.Repositories.SqlGenerator.Filters;

namespace Xbl.Data.Extensions;

internal static class ReadOnlyDapperRepositoryExtensions
{
    public static SqlQueryVisitor Visit<TEntity>(this IReadOnlyDapperRepository<TEntity> source, Expression expression)
        where TEntity : class
    {
        var visitor = new SqlQueryVisitor(PropertyMappings.GetMapping<TEntity>());
        visitor.Visit(expression);
        visitor.FilterData.SelectInfo ??= new SelectInfo { Columns = ["*"] };
        return visitor;
    }
}