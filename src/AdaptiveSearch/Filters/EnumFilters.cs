using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch.Filters
{

    public class EnumFilter<TEnum> : IAdaptiveFilter where TEnum : struct, Enum
    {
        public TEnum? Equal { get; set; }
        public TEnum? NotEqual { get; set; }
        public List<TEnum> In { get; set; } = new List<TEnum>();
        public List<TEnum> NotIn { get; set; } = new List<TEnum>();

        public bool HasValue => Equal.HasValue || NotEqual.HasValue || In.Any() || NotIn.Any();

        public Expression BuildExpression<TSource>(Expression property)
        {
            var expressions = new List<Expression>();

            if (Equal.HasValue)
                expressions.Add(Expression.Equal(property, Expression.Constant(Equal.Value)));

            if (NotEqual.HasValue)
                expressions.Add(Expression.NotEqual(property, Expression.Constant(NotEqual.Value)));

            if (In != null && In.Any())
            {
                var inValues = In.Select(v => Expression.Constant(v));
                var inExpression = inValues
                    .Select(v => Expression.Equal(property, v))
                    .Aggregate(Expression.OrElse);

                expressions.Add(inExpression);
            }

            if (NotIn != null && NotIn.Any())
            {
                var notInValues = NotIn.Select(v => Expression.Constant(v));
                var notInExpression = notInValues
                    .Select(v => Expression.NotEqual(property, v))
                    .Aggregate(Expression.AndAlso);

                expressions.Add(notInExpression);
            }

            // Combine all expressions with "AndAlso"
            var combined = expressions.Aggregate(Expression.AndAlso);
            return combined;
        }
    }
}