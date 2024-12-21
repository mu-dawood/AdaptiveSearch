using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch;
using AdaptiveSearch.Attributes;
using AdaptiveSearch.Interfaces;

namespace System.Linq
{

    public static class AdaptiveSearchExtensions
    {
        internal static bool IsSkip(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<SkipAttribute>() != null;
        internal static bool IsTake(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<TakeAttribute>() != null;


        public static AdaptiveSearch<TSource, TObject> AdaptiveSearch<TSource, TObject>(this IQueryable<TSource> source, TObject searchObject)
        {
            var specs = new PropertySpecifications<TSource>();
            if (searchObject == null) return new AdaptiveSearch<TSource, TObject>(source, searchObject, specs);
            Type targetType = searchObject.GetType();
            Type interfaceType = typeof(IAdaptiveFilter);
            var properties = targetType.GetProperties();
            foreach (var p in properties)
            {
                if (interfaceType.IsAssignableFrom(p.PropertyType))
                {
                    specs.FilterProperties.Enqueue(p);
                }
                else if (p.IsSkip())
                {
                    specs.SkipProperties.Enqueue(p);
                }
                else if (p.IsTake())
                {
                    specs.TakeProperties.Enqueue(p);
                }
                else
                {
                    specs.NonFilterProperties.Enqueue(p);
                }
            }
            return new AdaptiveSearch<TSource, TObject>(source, searchObject, specs);
        }


        /// <summary>
        /// Apply filter to the query.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <returns></returns>
        public static AdaptiveFilter<TSource, TFilter> AdaptiveSearch<TSource, TFilter>(this IQueryable<TSource> source, TFilter filter, Func<AdaptiveFilterConfiguration<TSource, TFilter>, AdaptiveFilter<TSource, TFilter>> config) where TFilter : IAdaptiveFilter?
        {
            return new AdaptiveFilter<TSource, TFilter>(source, filter).Configure(config);
        }

        /// <summary>
        /// Apply filter to the query.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="filter"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="filter"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>

        public static IQueryable<TSource> AdaptiveSearch<TSource, TFilter, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, TFilter filter) where TFilter : IAdaptiveFilter?
        {
            if (filter == null || !filter.HasValue()) return source;
            var parameter = selector.Parameters[0]; // Parameter (e.g., x)
            var property = selector.Body;
            if (property == null) return source;
            var expression = filter.BuildExpression<TSource>(property);
            return new AdaptiveFilter<TSource, TFilter>(source, filter).WithCExpression(parameter, expression);
        }

    }
}