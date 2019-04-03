using System.Linq.Expressions;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public interface IPropertyCopier
    {
        Expression GetCopyExpression(MemberExpression sourceProp, MemberExpression targetProp);
    }
}