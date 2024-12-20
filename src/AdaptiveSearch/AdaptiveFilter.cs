

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch
{
    public class AdaptiveFilter<TSource, TFilter> : IQueryable<TSource> where TFilter : IAdaptiveFilter
    {
        private readonly IQueryable<TSource> source;
        private readonly ParameterExpression? parameter;
        private readonly Expression? expression;
        private readonly TFilter filter;

        internal AdaptiveFilter(IQueryable<TSource> source, TFilter filter)
        {
            this.source = source;
            this.filter = filter;
        }

        private AdaptiveFilter(IQueryable<TSource> source, ParameterExpression parameter, Expression expression, TFilter filter)
        {
            this.source = source;
            this.parameter = parameter;
            this.filter = filter;
            this.expression = expression;
        }


        public Type ElementType => source.ElementType;

        public Expression Expression => Apply().Expression;

        public IQueryProvider Provider => Apply().Provider;

        public IEnumerator<TSource> GetEnumerator()
        {
            return Apply().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Apply().GetEnumerator();
        }

        /// <summary>
        /// Configure single property behavior.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="filter"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <returns></returns>
        public AdaptiveFilter<TSource, TFilter> Configure(
            Func<AdaptiveFilterConfiguration<TSource, TFilter>, AdaptiveFilter<TSource, TFilter>> config
        )
        {
            if (!filter.HasValue) return this;
            return config(new AdaptiveFilterConfiguration<TSource, TFilter>(filter, this));

        }

        internal AdaptiveFilter<TSource, TFilter> WithCExpression(ParameterExpression parameter,Expression expression)
        {
            return new AdaptiveFilter<TSource, TFilter>(source, parameter, expression, filter);
        }

        private IQueryable<TSource> Apply()
        {
            if (!filter.HasValue) return source;
            return source.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
        }

    }

}