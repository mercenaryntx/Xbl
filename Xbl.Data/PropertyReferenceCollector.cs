using System.Linq.Expressions;

namespace Xbl.Data;

public class PropertyReferenceCollector : ExpressionVisitor
{
    public HashSet<string> PropertyReferences { get; } = [];

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.MemberType == System.Reflection.MemberTypes.Property)
        {
            PropertyReferences.Add(node.Member.Name);
        }
        return base.VisitMember(node);
    }
}