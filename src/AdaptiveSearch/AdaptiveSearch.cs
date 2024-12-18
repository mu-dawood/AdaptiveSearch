using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Interfaces;

namespace System.Linq
{

    public static class AdaptiveSearchExtensions
    {

        public static IQueryable<TSource> AdaptiveSearch<TSource, TObject>(this IQueryable<TSource> source, TObject searchObject, bool applyAllProperties = false)
        {
            if (searchObject == null) return source;
            Type targetType = searchObject.GetType();
            Type interfaceType = typeof(IAdaptiveFilter);
            PropertyInfo[] properties = targetType.GetProperties()
            .Where(p => applyAllProperties || interfaceType.IsAssignableFrom(p.PropertyType))
            .ToArray();
            if (properties.Length == 0) return source;
            Type sourceType = searchObject.GetType();
            string[] sourceProperties = sourceType.GetProperties().Select((x) => x.Name).ToArray();

            var query = source;
            foreach (PropertyInfo property in properties)
            {
                if (!sourceProperties.Contains(property.Name))
                    throw new Exception($"Source type does not contain property `{property.Name}`");

                object propertyValue = property.GetValue(searchObject);

                if (propertyValue == null) continue;
                if (propertyValue is IAdaptiveFilter filter)
                {
                    if (!filter.HasValue) continue;

                    var parameter = Expression.Parameter(typeof(TSource), "x");
                    var selector = Expression.Property(parameter, property.Name);
                    var expression = filter.BuildExpression<TSource>(selector);
                    query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
                }
                else if (applyAllProperties)
                {
                    var parameter = Expression.Parameter(typeof(TSource), "x");
                    var selector = Expression.Property(parameter, property.Name);
                    var expression = Expression.Equal(selector, Expression.Constant(propertyValue));
                    query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
                }
            }
            return query;

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