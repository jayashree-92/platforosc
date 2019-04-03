using System.Linq.Expressions;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public class NonNullablePropertyCopier : IPropertyCopier
    {
        public Expression GetCopyExpression(MemberExpression sourceProp, MemberExpression targetProp)
        {
            var copyToTarget = Expression.Assign(targetProp, sourceProp);
            var targetHasNoValue = Expression.Equal(Expression.Default(targetProp.Type), targetProp);
            return Expression.IfThen(targetHasNoValue, copyToTarget);
        }
    }
}