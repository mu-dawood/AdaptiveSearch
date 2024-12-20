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


        public static AdaptiveSearch<TSource, TObject> AdaptiveSearch<TSource, TObject>(this IQueryable<TSource> source, TObject searchObject, bool applyAllProperties = false, bool applyPaging = false)
        {
            var specs = new PropertySpecifications();
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
                else if (applyAllProperties)
                {
                    specs.NonFilterProperties.Enqueue(p);
                }
            }
            var res = new AdaptiveSearch<TSource, TObject>(source, searchObject, specs).ApplyFilters().ApplyNonFilters();
            if (applyPaging) return res.ApplyPaging();
            return res;
        }

        /// <summary>
        /// Apply single property
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="filter"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <returns></returns>
        public static IQueryable<TSource> AdaptiveSearch<TSource, TProperty, TFilter>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, TFilter filter) where TFilter : IAdaptiveFilter
        {
            if (!filter.HasValue) return source;
            return new AdaptiveFilter<TSource, TFilter>(source, filter, ApplyType.Or).ApplyTo(selector);
        }

        /// <summary>
        /// Apply to multiple properties it will return or
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="filter"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <returns></returns>
        public static AdaptiveFilter<TSource, TFilter> AdaptiveSearch<TSource, TFilter>(this IQueryable<TSource> source, TFilter filter, ApplyType type) where TFilter : IAdaptiveFilter
        {
            return new AdaptiveFilter<TSource, TFilter>(source, filter, type); ;
        }

    }
}