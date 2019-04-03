using System.Linq.Expressions;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public class NullablePropertyCopier : IPropertyCopier
    {
        public Expression GetCopyExpression(MemberExpression sourceProp, MemberExpression targetProp)
        {
            var copyToTarget = Expression.Assign(targetProp, sourceProp);
            var sourceHasValue = Expression.NotEqual(Expression.Constant(null), sourceProp);
            var targetHasNoValue = Expression.Equal(Expression.Constant(null), targetProp);
            var shouldCopy = Expression.AndAlso(sourceHasValue, targetHasNoValue);
            return Expression.IfThen(shouldCopy, copyToTarget);
        }
    }
}