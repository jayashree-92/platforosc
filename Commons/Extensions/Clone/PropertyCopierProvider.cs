using System;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public static class PropertyCopierProvider
    {
        private static readonly IPropertyCopier nullablePropertyCopier = new NullablePropertyCopier();
        private static readonly IPropertyCopier nonNullablePropertyCopier = new NonNullablePropertyCopier();
        private static readonly IPropertyCopier stringPropertyCopier = new StringPropertyCopier();
        
        public static IPropertyCopier GetCopier(Type propertyType)
        {
            return CanBeNull(propertyType) ? nullablePropertyCopier : (propertyType == typeof (string) ? stringPropertyCopier : nonNullablePropertyCopier);
        }

        private static bool CanBeNull(Type sourceType)
        {
            return Nullable.GetUnderlyingType(sourceType) != null;
        }
    }
}