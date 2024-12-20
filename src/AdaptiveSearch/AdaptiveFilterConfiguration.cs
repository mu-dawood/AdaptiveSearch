using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch
{
    public class AdaptiveFilterConfiguration<TSource, TObject> where TObject : IAdaptiveFilter
    {
        private readonly TObject filter;
        private readonly IEnumerable<Func<ParameterExpression, Expression>> expressions;
        private readonly AdaptiveFilter<TSource, TObject> _source;

        internal AdaptiveFilterConfiguration(TObject filter, AdaptiveFilter<TSource, TObject> source)
        {
            this.filter = filter;
            expressions = new Func<ParameterExpression, Expression>[] { };
            _source = source;
        }

        private AdaptiveFilterConfiguration(TObject filter, IEnumerable<Func<ParameterExpression, Expression>> expressions, AdaptiveFilter<TSource, TObject> source)
        {
            this.filter = filter;
            this.expressions = expressions;
            _source = source;
        }

        /// <summary>
        /// Map filter to property.
        /// You can call this twice to map it to multiple properties.
        /// If you need to configure how multiple configuration works . call  [WithType] method.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>

        public AdaptiveFilterConfiguration<TSource, TObject> MapTo<TProperty>(Expression<Func<TSource, TProperty>> selector)
        {
            var body = selector.Body;
            var propInfo = selector.GetPropertyOfType();
            var res = expressions.Append((p) =>
            {
                var selector = Expression.Property(p, propInfo.Name);
                return filter.BuildExpression<TSource>(selector);
            });
            return new AdaptiveFilterConfiguration<TSource, TObject>(filter, res, _source);
        }

        /// <summary>
        /// Configure how to handle multiple Mapping
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AdaptiveFilter<TSource, TObject> WithType(ConfigurationType type)
        {
            if (expressions.Count() == 0) return _source;
            var exps = new List<Expression>();
            var parameter = Expression.Parameter(typeof(TSource), "x");
            foreach (var e in expressions)
            {
                exps.Add(e(parameter));
            }
            var combined = type == ConfigurationType.Or ? exps.Aggregate(Expression.OrElse) : exps.Aggregate(Expression.AndAlso);
            return _source.WithCExpression(parameter, combined);
        }

        public static implicit operator AdaptiveFilter<TSource, TObject>(AdaptiveFilterConfiguration<TSource, TObject> configuration)
        {
            return configuration.WithType(ConfigurationType.And);
        }
    }


}

