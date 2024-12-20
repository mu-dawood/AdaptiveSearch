using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch
{
    public enum ConfigurationType { And, Or }
    public class AdaptiveSearchConfiguration<TSource, TObject, TFilter> where TFilter : IAdaptiveFilter
    {
        private readonly TFilter filter;
        private readonly PropertyInfo propertyInfo;
        private readonly IEnumerable<Func<ParameterExpression, Expression>> expressions;
        private readonly AdaptiveSearch<TSource, TObject> _source;

        internal AdaptiveSearchConfiguration(PropertyInfo propertyInfo, TFilter filter, AdaptiveSearch<TSource, TObject> source)
        {
            this.filter = filter;
            expressions = new Func<ParameterExpression, Expression>[] { };
            _source = source;
            this.propertyInfo = propertyInfo;
        }

        private AdaptiveSearchConfiguration(PropertyInfo propertyInfo, TFilter filter, IEnumerable<Func<ParameterExpression, Expression>> expressions, AdaptiveSearch<TSource, TObject> source)
        {
            this.filter = filter;
            this.expressions = expressions;
            _source = source;
            this.propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Map filter to property.
        /// You can call this twice to map it to multiple properties.
        /// If you need to configure how multiple configuration works . call  [WithType] method.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>

        public AdaptiveSearchConfiguration<TSource, TObject, TFilter> MapTo<TProperty>(Expression<Func<TSource, TProperty>> selector)
        {
            var body = selector.Body;
            var propInfo = selector.GetPropertyOfType();
            var res = expressions.Append((p) =>
            {
                var selector = Expression.Property(p, propInfo.Name);
                return filter.BuildExpression<TSource>(selector);
            });
            return new AdaptiveSearchConfiguration<TSource, TObject, TFilter>(propertyInfo, filter, res, _source);
        }

        /// <summary>
        /// Configure how to handle multiple Mapping
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AdaptiveSearch<TSource, TObject> WithType(ConfigurationType type)
        {
            if (expressions.Count() == 0) return _source;
            var exps = new List<Expression>();
            var parameter = Expression.Parameter(typeof(TSource), "x");
            foreach (var e in expressions)
            {
                exps.Add(e(parameter));
            }
            var combined = type == ConfigurationType.Or ? exps.Aggregate(Expression.OrElse) : exps.Aggregate(Expression.AndAlso);
            return _source.WithCustomExpression(propertyInfo.Name, combined);

        }

        public static implicit operator AdaptiveSearch<TSource, TObject>(AdaptiveSearchConfiguration<TSource, TObject, TFilter> configuration)
        {
            return configuration.WithType(ConfigurationType.And);
        }

    }


}

