using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch.Filters
{

    public class ComparableFilter<T> : IAdaptiveFilter where T : struct, IComparable<T>
    {
        public T? Equal { get; set; }
        public T? NotEqual { get; set; }
        public T? GreaterThan { get; set; }
        public T? GreaterThanOrEqual { get; set; }
        public T? LessThan { get; set; }
        public T? LessThanOrEqual { get; set; }

        public bool HasValue => Equal.HasValue || NotEqual.HasValue || GreaterThan.HasValue || GreaterThanOrEqual.HasValue || LessThan.HasValue || LessThanOrEqual.HasValue;

        public Expression BuildExpression<TSource>(Expression property)
        {
            var expressions = new List<Expression>();

            // Equal
            if (Equal.HasValue)
                expressions.Add(Expression.Equal(property, Expression.Constant(Equal.Value)));

            // NotEqual
            if (NotEqual.HasValue)
                expressions.Add(Expression.NotEqual(property, Expression.Constant(NotEqual.Value)));

            // GreaterThan
            if (GreaterThan.HasValue)
                expressions.Add(Expression.GreaterThan(property, Expression.Constant(GreaterThan.Value)));

            // GreaterThanOrEqual
            if (GreaterThanOrEqual.HasValue)
                expressions.Add(Expression.GreaterThanOrEqual(property, Expression.Constant(GreaterThanOrEqual.Value)));

            // LessThan
            if (LessThan.HasValue)
                expressions.Add(Expression.LessThan(property, Expression.Constant(LessThan.Value)));

            // LessThanOrEqual
            if (LessThanOrEqual.HasValue)
                expressions.Add(Expression.LessThanOrEqual(property, Expression.Constant(LessThanOrEqual.Value)));

            // Combine all expressions with "AndAlso"
            var combined = expressions.Aggregate(Expression.AndAlso);
            return combined;
        }
    }



}
