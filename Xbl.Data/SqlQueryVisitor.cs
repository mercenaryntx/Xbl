using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.SqlGenerator.Filters;

namespace Xbl.Data;

internal class SqlQueryVisitor(Dictionary<string, SqlPropertyMetadata> propertyMapping = null) : ExpressionVisitor
{
    private readonly Dictionary<string, SqlPropertyMetadata> _propertyMapping = propertyMapping ?? new Dictionary<string, SqlPropertyMetadata>();

    public LambdaExpression Where { get; private set; }
    public FilterData FilterData { get; } = new();
    public bool UsedPropertyMapping { get; private set; }
    public string MethodName { get; private set; }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        MethodName ??= node.Method.Name;
        switch (node.Method.Name)
        {
            case nameof(Queryable.Any):
            case nameof(Queryable.Count):
            case nameof(Queryable.Where):
            case nameof(Queryable.First):
            case nameof(Queryable.FirstOrDefault):
            case nameof(Queryable.Single):
            case nameof(Queryable.SingleOrDefault):
                var where = GetLambda(node);
                if (where != null)
                {
                    Where = Where == null ? where : MergeWheres(Where, where);
                }

                if (node.Method.Name == nameof(Queryable.Count))
                {
                    SetSelectInfo(["COUNT(*)"]);
                }
                break;
            case nameof(Queryable.OrderBy):
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Direction = OrderInfo.SortDirection.ASC;
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Insert(0, GetPropertyName(node));
                break;
            case nameof(Queryable.OrderByDescending):
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Direction = OrderInfo.SortDirection.DESC;
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Insert(0, GetPropertyName(node));
                break;
            case nameof(Queryable.ThenBy):
            case nameof(Queryable.ThenByDescending):
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Add(GetPropertyName(node));
                break;
            case nameof(Queryable.Take):
                FilterData.LimitInfo ??= new LimitInfo();
                FilterData.LimitInfo.Limit = GetIntegerArgument(node);
                break;
            case nameof(Queryable.Skip):
                FilterData.LimitInfo ??= new LimitInfo { Limit = uint.MaxValue };
                FilterData.LimitInfo.Offset = GetIntegerArgument(node);
                break;
            case nameof(Queryable.DistinctBy):
                UsedPropertyMapping = true;
                var propertyName = GetPropertyName(node);
                if (_propertyMapping.TryGetValue(propertyName, out var column))
                {
                    SetSelectInfo([$"DISTINCT {MapColumn(column)}"]);
                }
                break;
            case nameof(Queryable.Select):
                UsedPropertyMapping = true;
                var collector = new PropertyReferenceCollector();
                collector.Visit(node.Arguments[1]);
                SetSelectInfo(collector.PropertyReferences.Select(p => MapColumn(_propertyMapping[p])).ToList());
                break;
            default:
                throw new NotSupportedException("Method not supported: " + node.Method.Name);
        }
        return base.VisitMethodCall(node);
    }

    private static LambdaExpression MergeWheres(LambdaExpression left, LambdaExpression right)
    {
        return Expression.Lambda(Expression.AndAlso(left.Body, right.Body), left.Parameters);
    }

    private static LambdaExpression GetLambda(MethodCallExpression node)
    {
        if (node.Arguments.Count < 2) return null;
        if (node.Arguments[1] is not UnaryExpression unary) return null;
        return (LambdaExpression) unary.Operand;
    }

    private static string GetPropertyName(MethodCallExpression node)
    {
        var lambda = GetLambda(node);
        var body = (MemberExpression)lambda.Body;
        return body.Member.Name;
    }

    private static uint GetIntegerArgument(MethodCallExpression node)
    {
        var constant = (ConstantExpression)node.Arguments[1];
        return Convert.ToUInt32(constant.Value);
    }

    private static string MapColumn(SqlPropertyMetadata column)
    {
        var selection = column.ColumnName;
        if (!string.IsNullOrEmpty(column.Alias))
        {
            selection += $" AS {column.Alias}";
        }
        return selection;
    }

    private void SetSelectInfo(List<string> columns)
    {
        if (FilterData.SelectInfo != null) return;
        FilterData.SelectInfo = new SelectInfo
        {
            Columns = columns
        };
    }
}