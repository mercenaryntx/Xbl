using System.Collections;
using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories;

namespace Xbl.Data;

public class DapperQueryable<TEntity, TProjection> : IOrderedQueryable<TProjection> where TEntity : class
{
    private readonly DapperProvider<TEntity, TProjection> _provider;

    public DapperQueryable(IReadOnlyDapperRepository<TEntity> inner, Expression expression = null)
    {
        _provider = new DapperProvider<TEntity, TProjection>(inner, expression ?? Expression.Constant(this));
    }

    public Type ElementType => typeof(TEntity);

    public Expression Expression => _provider.Expression;

    public IQueryProvider Provider => _provider;

    public IEnumerator<TProjection> GetEnumerator()
    {
        return _provider.Execute<IEnumerable<TProjection>>(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}