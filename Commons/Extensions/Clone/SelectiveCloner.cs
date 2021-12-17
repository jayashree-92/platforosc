using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Com.HedgeMark.Commons.Extensions.Clone
{
    public static class SelectiveCloner
    {
        static readonly ConcurrentDictionary<string, object> cachedExpressions = new ConcurrentDictionary<string, object>();

        public static void CopyFrom<T>(this T target, T source, IEnumerable<string> propertiesToIgnore = null)
        {
            var typeName = typeof (T).Name;
            Action<T, T> copyExpression;
            if (!cachedExpressions.TryGetValue(typeName, out var customCopyExpression))
            {
                copyExpression = CopyFromInternal<T>(propertiesToIgnore);
                cachedExpressions.AddOrUpdate(typeName, copyExpression, (s, o) => copyExpression);
            }
            else
                copyExpression = ((Action<T, T>) customCopyExpression);
            copyExpression(source, target);
        }

        private static bool IsNotMapped(IEnumerable<CustomAttributeData> attributes)
        {
            return attributes.Any(data => data.AttributeType == typeof(NotMappedAttribute));
        }

        private static bool ShouldCopyProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.CanWrite && !(propertyInfo.GetMethod.IsVirtual || IsNotMapped(propertyInfo.CustomAttributes));
        }

        private static Action<T, T> CopyFromInternal<T>(IEnumerable<string> propertiesToIgnore)
        {
            var type = typeof(T);
            var properties = type.GetProperties().Where(ShouldCopyProperty).ToList();
            if (propertiesToIgnore != null)
                properties = properties.Where(x => !propertiesToIgnore.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)).ToList();

            var sourceParam = Expression.Parameter(type, "sourceParam");
            var targetParam = Expression.Parameter(type, "targetParam");
            var copyStatements = new List<Expression>();

            properties.ForEach(prop =>
            {
                var sourceProp = Expression.Property(sourceParam, prop);
                var targetProp = Expression.Property(targetParam, prop);
                var propertyCopier = PropertyCopierProvider.GetCopier(prop.PropertyType);
                copyStatements.Add(propertyCopier.GetCopyExpression(sourceProp,targetProp));
            });

            var codeBlock = Expression.Block(copyStatements);
            var lambda = Expression.Lambda<Action<T, T>>(codeBlock, sourceParam, targetParam);
            return lambda.Compile();
        }
    }
}