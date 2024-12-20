

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Attributes;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch
{
    public class AdaptiveSearch<TSource, TObject> : IQueryable<TSource>
    {
        private readonly IQueryable<TSource> source;
        private readonly PropertySpecifications properties;
        private readonly TObject searchObject;
        internal AdaptiveSearch(IQueryable<TSource> source, TObject searchObject, PropertySpecifications properties)
        {
            this.source = source;
            this.properties = properties;
            this.searchObject = searchObject;
        }

        public Type ElementType => source.ElementType;

        public Expression Expression => source.Expression;

        public IQueryProvider Provider => source.Provider;

        public IEnumerator<TSource> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        internal AdaptiveSearch<TSource, TObject> ApplyFilters()
        {
            if (properties.FilterProperties.Count == 0) return this;
            var query = source;
            while (properties.FilterProperties.Count > 0)
            {
                var prop = properties.FilterProperties.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (typeof(TSource).GetProperty(prop.Name) == null)
                    throw new Exception($"Source type does not contain property `{prop.Name}`");
                if (!(propertyValue is IAdaptiveFilter filter) || !filter.HasValue) continue;
                var parameter = Expression.Parameter(typeof(TSource), "x");
                var selector = Expression.Property(parameter, prop.Name);
                var expression = filter.BuildExpression<TSource>(selector);
                query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties);
        }

        internal AdaptiveSearch<TSource, TObject> ApplyNonFilters()
        {
            if (properties.NonFilterProperties.Count == 0) return this;
            var query = source;
            while (properties.NonFilterProperties.Count > 0)
            {
                var prop = properties.NonFilterProperties.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (typeof(TSource).GetProperty(prop.Name) == null)
                    throw new Exception($"Source type does not contain property `{prop.Name}`");

                var parameter = Expression.Parameter(typeof(TSource), "x");
                var selector = Expression.Property(parameter, prop.Name);
                var expression = Expression.Equal(selector, Expression.Constant(propertyValue));
                query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties);
        }


        /// <summary>
        /// Apply only skip properties to the query.
        /// Skip property is each property that decorated by `SkipAttribute` annotation.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if the property not of type int</exception>
        /// <summary>
        /// <see cref="SkipAttribute"/>
        /// </summary>
        /// <returns></returns>
        public AdaptiveSearch<TSource, TObject> ApplySkip()
        {
            if (properties.SkipProperties.Count == 0) return this;
            var query = source;
            while (properties.SkipProperties.Count > 0)
            {
                var prop = properties.SkipProperties.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (!(propertyValue is int skip))
                    throw new ArgumentException("Skip property must be of type int");
                query = query.Skip(skip);
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties);
        }

        /// <summary>
        /// Apply only take properties to the query.
        /// Take property is each property that decorated by `TakeAttribute` annotation.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if the property not of type int</exception>
        /// <summary>
        /// <see cref="TakeAttribute"/>
        /// </summary>
        public AdaptiveSearch<TSource, TObject> ApplyTake()
        {
            if (properties.TakeProperties.Count == 0) return this;
            var query = source;
            while (properties.TakeProperties.Count > 0)
            {
                var prop = properties.TakeProperties.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (!(propertyValue is int skip))
                    throw new ArgumentException("Skip property must be of type int");
                query = query.Take(skip);
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties);
        }

        /// <summary>
        /// Apply both skip and take properties to the query.
        /// </summary>
        /// <returns></returns>
        /// <see cref="ApplySkip"/>
        /// <see cref="ApplyTake"/>
        public AdaptiveSearch<TSource, TObject> ApplyPaging()
        {
            return ApplySkip().ApplyTake();
        }
    }


    internal class PropertySpecifications
    {
        private Queue<PropertyInfo>? skipProperties;
        private Queue<PropertyInfo>? takeProperties;
        private Queue<PropertyInfo>? filterProperties;
        private Queue<PropertyInfo>? nonFilterProperties;
        internal Queue<PropertyInfo> NonFilterProperties => nonFilterProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> FilterProperties => filterProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> SkipProperties => skipProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> TakeProperties => takeProperties ??= new Queue<PropertyInfo>();

    }
}