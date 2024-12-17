using System;
using System.Linq.Expressions;

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

        
    }
}
