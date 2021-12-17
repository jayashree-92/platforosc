using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Com.HedgeMark.Commons.Extensions
{
    internal class Cloner
    {
        static readonly ConcurrentDictionary<string, object> cachedExpressions = new ConcurrentDictionary<string, object>();

        public static T DeepCopy<T>(T obj)
        {
            var typeName = typeof(T).FullName;
            if (!cachedExpressions.TryGetValue(typeName, out var cloneExpression))
            {
                var deepCopy = DeepCopy<T>();
                cachedExpressions.AddOrUpdate(typeName, deepCopy, (s, o) => deepCopy);
                return deepCopy(obj);
            }
            return ((Func<T, T>)cloneExpression)(obj);
        }

        private static Func<T, T> DeepCopy<T>()
        {
            var type = typeof(T);
            var source = Expression.Parameter(type, "src");
            var memberBindings = (from prop in type.GetProperties().Where(info => !info.GetGetMethod().IsVirtual && info.SetMethod != null)
                                  let memberExpression = Expression.Property(source, prop)
                                  select Expression.Bind(prop, memberExpression)).Cast<MemberBinding>().ToList();

            var codeBlock = Expression.MemberInit(Expression.New(type), memberBindings);
            var lambda = Expression.Lambda<Func<T, T>>(codeBlock, source);
            return lambda.Compile();
        }
    }
}