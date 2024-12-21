using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch.Filters
{

    public class StringFilter : IAdaptiveFilter
    {
        public string? Equal { get; set; }
        public string? StartsWith { get; set; }
        public string? EndsWith { get; set; }
        public string? Contains { get; set; }
        public string? NotEqual { get; set; }
        public string? NotStartsWith { get; set; }
        public string? NotEndsWith { get; set; }
        public string? NotContains { get; set; }
        public bool? IsNullOrEmpty { get; set; }

        public bool HasValue ()=> IsNullOrEmpty.HasValue || !string.IsNullOrWhiteSpace(Equal) || !string.IsNullOrWhiteSpace(StartsWith) || !string.IsNullOrWhiteSpace(EndsWith) || !string.IsNullOrWhiteSpace(Contains) || !string.IsNullOrWhiteSpace(NotEqual) || !string.IsNullOrWhiteSpace(NotStartsWith) || !string.IsNullOrWhiteSpace(NotEndsWith) || !string.IsNullOrWhiteSpace(NotContains);

        public Expression BuildExpression<TSource>(Expression property)
        {


            var expressions = new List<Expression>();

            if (!string.IsNullOrEmpty(Equal))
                expressions.Add(Expression.Equal(property, Expression.Constant(Equal)));

            if (!string.IsNullOrEmpty(NotEqual))
                expressions.Add(Expression.NotEqual(property, Expression.Constant(NotEqual)));

            if (!string.IsNullOrEmpty(StartsWith))
                expressions.Add(property.GenerateMethodCall("StartsWith", StartsWith));

            if (!string.IsNullOrEmpty(NotStartsWith))
                expressions.Add(Expression.Not(property.GenerateMethodCall("StartsWith", NotStartsWith)));

            if (!string.IsNullOrEmpty(EndsWith))
                expressions.Add(property.GenerateMethodCall("EndsWith", EndsWith));

            if (!string.IsNullOrEmpty(NotEndsWith))
                expressions.Add(Expression.Not(property.GenerateMethodCall("EndsWith", NotEndsWith)));

            if (!string.IsNullOrEmpty(Contains))
                expressions.Add(property.GenerateMethodCall("Contains", Contains));

            if (!string.IsNullOrEmpty(NotContains))
                expressions.Add(Expression.Not(property.GenerateMethodCall("Contains", NotContains)));

            if (IsNullOrEmpty.HasValue)
            {
                var isNullOrEmptyMethod = typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) });
                var call = Expression.Call(isNullOrEmptyMethod, property);
                if (IsNullOrEmpty.Value) expressions.Add(call);
                else expressions.Add(Expression.Not(call));
                
            }

            // Combine all expressions with "AndAlso"
            Expression combined = expressions.Aggregate(Expression.AndAlso);

            return combined;
        }
    }
}