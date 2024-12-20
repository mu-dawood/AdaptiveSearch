using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AdaptiveSearch
{

    internal static class Utils
    {
        internal static Expression AddTo(this Expression expression, Expression oldExpression)
        {
            if (oldExpression == null) return expression;
            return Expression.AndAlso(oldExpression, expression);
        }

        internal static MethodCallExpression GenerateMethodCall<T>(this Expression property, string methodName, T value)
        {
            var method = typeof(T).GetMethod(methodName, new[] { typeof(T) });
            var constant = Expression.Constant(value, typeof(T));
            return Expression.Call(property, method, constant);
        }

        internal static PropertyInfo GetPropertyOfType<TSource, TProperty>(this Expression<Func<TSource, TProperty>> selector)
        {
            var body = selector.Body;
            if (!(body is MemberExpression member))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.", body.ToString()));
            }
            if (!(member.Member is PropertyInfo propInfo))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.", body.ToString()));
            }
            Type sourceType = typeof(TSource);
            if (propInfo.ReflectedType != null && sourceType != propInfo.ReflectedType && !sourceType.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.", body.ToString(),
                    sourceType));
            }
            return propInfo;
        }



    }
}
