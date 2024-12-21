using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch.Filters
{

    public class BooleanFilter : IAdaptiveFilter
    {
        public bool? Equal { get; set; }
        public bool? NotEqual { get; set; }

        public bool HasValue ()=> Equal.HasValue || NotEqual.HasValue;

        public Expression BuildExpression<TSource>(Expression property)
        {
            var expressions = new List<Expression>();

            // Equal
            if (Equal.HasValue)
                expressions.Add(Expression.Equal(property, Expression.Constant(Equal.Value)));

            // NotEqual
            if (NotEqual.HasValue)
                expressions.Add(Expression.NotEqual(property, Expression.Constant(NotEqual.Value)));


            // Combine all expressions with "AndAlso"
            var combined = expressions.Aggregate(Expression.AndAlso);
            return combined;
        }
    }
}