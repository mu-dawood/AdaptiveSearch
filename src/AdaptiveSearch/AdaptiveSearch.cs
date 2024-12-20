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


        public static IEnumerable<TSource> AdaptiveSearch<TSource, TProperty, TFilter>(this IEnumerable<TSource> source, Expression<Func<TSource, TProperty>> selector, TFilter filter) where TFilter : IAdaptiveFilter
        {
            if (!filter.HasValue) return source;
            var parameter = selector.Parameters[0]; // Parameter (e.g., x)
            var property = selector.Body;
            if (property == null) return source;
            var expression = filter.BuildExpression<TSource>(property);
            return source.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter).Compile());
        }

        public static IQueryable<TSource> AdaptiveSearch<TSource, TProperty, TFilter>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, TFilter filter) where TFilter : IAdaptiveFilter
        {
            if (!filter.HasValue) return source;
            var parameter = selector.Parameters[0]; // Parameter (e.g., x)
            var property = selector.Body;
            if (property == null) return source;
            var expression = filter.BuildExpression<TSource>(property);

            return source.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
        }

    }
}