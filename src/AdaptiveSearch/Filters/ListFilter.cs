using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch.Filters
{

    public class ListFilter<T> : IAdaptiveFilter
    {
        public IEnumerable<T> ContainsAny { get; set; } = new List<T>();
        public IEnumerable<T> ContainsAll { get; set; } = new List<T>();
        public IEnumerable<T> DoesNotContainAny { get; set; } = new List<T>();
        public IEnumerable<T> DoesNotContainAll { get; set; } = new List<T>();

        public bool HasValue ()=> ContainsAny.Any() || ContainsAll.Any() || DoesNotContainAny.Any() || DoesNotContainAll.Any();

        public Expression BuildExpression<TSource>(Expression property)
        {
            var expressions = new List<Expression>();

            // ContainsAny: list.Any(x => filterList.Contains(x))
            if (ContainsAny != null && ContainsAny.Any())
            {
                var containsAnyExpr = Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Any),
                    new[] { typeof(T) },
                    property,
                    CreateLambdaContains(ContainsAny)
                );
                expressions.Add(containsAnyExpr);
            }

            // ContainsAll: filterList.All(x => list.Contains(x))
            if (ContainsAll != null && ContainsAll.Any())
            {
                var containsAllExpr = Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.All),
                    new[] { typeof(T) },
                    Expression.Constant(ContainsAll),
                    CreateLambdaContainsForAll(property)
                );
                expressions.Add(containsAllExpr);
            }

            // DoesNotContainAny: !list.Any(x => filterList.Contains(x))
            if (DoesNotContainAny != null && DoesNotContainAny.Any())
            {
                var doesNotContainAnyExpr = Expression.Not(
                    Expression.Call(
                        typeof(Enumerable),
                        nameof(Enumerable.Any),
                        new[] { typeof(T) },
                        property,
                        CreateLambdaContains(DoesNotContainAny)
                    )
                );
                expressions.Add(doesNotContainAnyExpr);
            }

            // DoesNotContainAll: !filterList.All(x => list.Contains(x))
            if (DoesNotContainAll != null && DoesNotContainAll.Any())
            {
                var doesNotContainAllExpr = Expression.Not(
                    Expression.Call(
                        typeof(Enumerable),
                        nameof(Enumerable.All),
                        new[] { typeof(T) },
                        Expression.Constant(DoesNotContainAll),
                        CreateLambdaContainsForAll(property)
                    )
                );
                expressions.Add(doesNotContainAllExpr);
            }
            // Combine all expressions with "AndAlso"
            var combined = expressions.Aggregate(Expression.AndAlso);
            return combined;
        }


        private Expression CreateLambdaContains(IEnumerable<T> values)
        {
            var valueParameter = Expression.Parameter(typeof(T), "x");
            var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
               .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);
            containsMethod = containsMethod.MakeGenericMethod(typeof(T));
            var call = Expression.Call(containsMethod, Expression.Constant(values), valueParameter);
            return Expression.Lambda(call, valueParameter);
        }

        private static Expression CreateLambdaContainsForAll(Expression property)
        {
            var valueParameter = Expression.Parameter(typeof(T), "x");
            var containsMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
             .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);
            containsMethod = containsMethod.MakeGenericMethod(typeof(T));
            var call = Expression.Call(containsMethod, property, valueParameter);
            return Expression.Lambda(call, valueParameter);
        }
    }
}