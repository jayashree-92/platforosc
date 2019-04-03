using System.Linq.Expressions;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public class StringPropertyCopier : IPropertyCopier
    {
        public Expression GetCopyExpression(MemberExpression sourceProp, MemberExpression targetProp)
        {
            var copyToTarget = Expression.Assign(targetProp, sourceProp);
            var sourceIsNotNull = Expression.NotEqual(Expression.Constant(null), sourceProp);
            var sourceIsNotEmpty = Expression.NotEqual(Expression.Constant(string.Empty), sourceProp);
            var sourceHasValue = Expression.AndAlso(sourceIsNotNull, sourceIsNotEmpty);

            var targetIsNull = Expression.Equal(Expression.Constant(null), targetProp);
            var targetIsEmpty = Expression.Equal(Expression.Constant(string.Empty), targetProp);
            var targetHasNoValue = Expression.OrElse(targetIsNull, targetIsEmpty);

            var shouldCopy = Expression.AndAlso(sourceHasValue, targetHasNoValue);
            return Expression.IfThen(shouldCopy, copyToTarget);
        }
    }
}