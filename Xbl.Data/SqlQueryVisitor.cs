using System.Linq.Expressions;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.SqlGenerator.Filters;

namespace Xbl.Data;

internal class SqlQueryVisitor : ExpressionVisitor
{
    private readonly Dictionary<string, SqlPropertyMetadata> _propertyMapping;

    public LambdaExpression Where { get; private set; }
    public FilterData FilterData { get; } = new();

    public SqlQueryVisitor(Dictionary<string, SqlPropertyMetadata> propertyMapping = null)
    {
        _propertyMapping = propertyMapping ?? new Dictionary<string, SqlPropertyMetadata>();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        switch (node.Method.Name)
        {
            case "Any":
            case "Count":
            case "Where":
            case "First":
            case "FirstOrDefault":
            case "Single":
            case "SingleOrDefault":
                var where = GetLambda(node);
                if (where != null)
                {
                    Where = Where == null ? where : MergeWheres(Where, where);
                }
                break;
            case "OrderBy":
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Direction = OrderInfo.SortDirection.ASC;
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Insert(0, GetPropertyName(node));
                break;
            case "OrderByDescending":
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Direction = OrderInfo.SortDirection.DESC;
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Insert(0, GetPropertyName(node));
                break;
            case "ThenBy":
            case "ThenByDescending":
                FilterData.OrderInfo ??= new OrderInfo();
                FilterData.OrderInfo.Columns ??= [];
                FilterData.OrderInfo.Columns.Add(GetPropertyName(node));
                break;
            case "Take":
                FilterData.LimitInfo ??= new LimitInfo();
                FilterData.LimitInfo.Limit = GetIntegerArgument(node);
                break;
            case "Skip":
                FilterData.LimitInfo ??= new LimitInfo { Limit = uint.MaxValue };
                FilterData.LimitInfo.Offset = GetIntegerArgument(node);
                break;
            case "DistinctBy":
                var propertyName = GetPropertyName(node);
                if (_propertyMapping.TryGetValue(propertyName, out var column))
                {
                    var selection = $"DISTINCT {column.ColumnName}";
                    if (!string.IsNullOrEmpty(column.Alias))
                    {
                        selection += $" AS {column.Alias}";
                    }
                    FilterData.SelectInfo ??= new SelectInfo();
                    FilterData.SelectInfo.Columns = [selection];
                }
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
}