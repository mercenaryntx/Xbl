using System.Collections;
using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories;

namespace Xbl.Data;

public class DapperQueryable<TEntity> : IOrderedQueryable<TEntity> where TEntity : class
{
    private readonly DapperProvider<TEntity> _provider;

    public DapperQueryable(IReadOnlyDapperRepository<TEntity> inner, Expression expression = null)
    {
        _provider = new DapperProvider<TEntity>(inner, expression ?? Expression.Constant(this));
    }

    public Type ElementType => typeof(TEntity);

    public Expression Expression => _provider.Expression;

    public IQueryProvider Provider => _provider;

    public IEnumerator<TEntity> GetEnumerator()
    {
        return _provider.Execute<IEnumerable<TEntity>>(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}