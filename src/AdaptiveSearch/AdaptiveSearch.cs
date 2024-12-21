

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
        private readonly PropertySpecifications<TSource> properties;
        private readonly TObject searchObject;
        internal AdaptiveSearch(IQueryable<TSource> source, TObject searchObject, PropertySpecifications<TSource> properties)
        {
            this.source = source;
            this.properties = properties;
            this.searchObject = searchObject;
        }

        public Type ElementType => source.ElementType;

        public Expression Expression => Apply().Expression;

        public IQueryProvider Provider => Apply().Provider;

        public IEnumerator<TSource> GetEnumerator()
        {
            return Apply(true).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Apply().GetEnumerator();
        }

        internal AdaptiveSearch<TSource, TObject> ApplyFilters()
        {
            if (properties.FilterProperties.Count == 0) return this;
            var queue = new Queue<PropertyInfo>(properties.FilterProperties);
            var query = source;
            while (queue.Count > 0)
            {
                var prop = queue.Dequeue();
                if (prop == null) continue;
                if (properties.CustomExpressions.ContainsKey(prop.Name))
                {
                    query = properties.CustomExpressions[prop.Name](query);
                    continue;
                }
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (typeof(TSource).GetProperty(prop.Name) == null)
                    throw new Exception($"Source type does not contain property `{prop.Name}`");
                if (!(propertyValue is IAdaptiveFilter filter) || !filter.HasValue()) continue;
                var parameter = Expression.Parameter(typeof(TSource), "x");
                var selector = Expression.Property(parameter, prop.Name);
                var expression = filter.BuildExpression<TSource>(selector);
                query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties.CopyWith(filterProperties: queue));
        }

        internal AdaptiveSearch<TSource, TObject> ApplyNonFilters()
        {
            if (properties.NonFilterProperties.Count == 0 || !properties.AllowNonFilterProperties) return this;
            var queue = new Queue<PropertyInfo>(properties.NonFilterProperties);
            var query = source;
            while (queue.Count > 0)
            {
                var prop = queue.Dequeue();
                if (prop == null) continue;
                if (properties.CustomExpressions.ContainsKey(prop.Name))
                {
                    query = properties.CustomExpressions[prop.Name](query);
                    continue;
                }
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (typeof(TSource).GetProperty(prop.Name) == null)
                    throw new Exception($"Source type does not contain property `{prop.Name}`");

                var parameter = Expression.Parameter(typeof(TSource), "x");
                var selector = Expression.Property(parameter, prop.Name);
                var expression = Expression.Equal(selector, Expression.Constant(propertyValue));
                query = query.Where(Expression.Lambda<Func<TSource, bool>>(expression, parameter));
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties.CopyWith(nonFilterProperties: queue));
        }


        /// <summary>
        /// Allow any property that is not delivered from `IAdaptiveFilter` interface.
        /// In this case we will match equality
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        public AdaptiveSearch<TSource, TObject> AllowAllProperties()
        {
            return new AdaptiveSearch<TSource, TObject>(source, searchObject, properties.CopyWith(allowNonFilterProperties: true));
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
            var queue = new Queue<PropertyInfo>(properties.SkipProperties);
            var query = source;
            while (queue.Count > 0)
            {
                var prop = queue.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (!(propertyValue is int skip))
                    throw new ArgumentException("Skip property must be of type int");
                query = query.Skip(skip);
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties.CopyWith(skipProperties: queue));
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
            var queue = new Queue<PropertyInfo>(properties.TakeProperties);
            var query = source;
            while (queue.Count > 0)
            {
                var prop = queue.Dequeue();
                if (prop == null) continue;
                var propertyValue = prop.GetValue(searchObject);
                if (propertyValue == null) continue;
                if (!(propertyValue is int skip))
                    throw new ArgumentException("Skip property must be of type int");
                query = query.Take(skip);
            }
            return new AdaptiveSearch<TSource, TObject>(query, searchObject, properties.CopyWith(takeProperties: queue));
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
        public AdaptiveSearch<TSource, TObject> Configure<TFilter>(
            Expression<Func<TObject, TFilter>> selector,
            Func<AdaptiveSearchConfiguration<TSource, TObject, TFilter>, AdaptiveSearch<TSource, TObject>> config
        )
        where TFilter : IAdaptiveFilter?
        {
            var property = selector.GetPropertyOfType();
            if (property.GetValue(searchObject) is TFilter filter)
            {
                if (filter.HasValue())
                {
                    return config(new AdaptiveSearchConfiguration<TSource, TObject, TFilter>(property, filter, this));
                }
                else return this;
            }
            else
                return this;
        }


        public AdaptiveSearchConfiguration<TSource, TObject, TFilter> Configure<TFilter>(Expression<Func<TObject, TFilter>> selector)
          where TFilter : IAdaptiveFilter?
        {
            var property = selector.GetPropertyOfType();
            if (property.GetValue(searchObject) is TFilter filter)
            {
                return new AdaptiveSearchConfiguration<TSource, TObject, TFilter>(property, filter, this);
            }
            else
                return new AdaptiveSearchConfiguration<TSource, TObject, TFilter>(property, default, this);
        }

        internal AdaptiveSearch<TSource, TObject> WithCustomExpression(string propertyName, Func<IQueryable<TSource>, IQueryable<TSource>> expression)
        {
            var customExpressions = properties.CustomExpressions.ToDictionary((entry) => entry.Key, (entry) => entry.Value);
            customExpressions[propertyName] = expression;
            return new AdaptiveSearch<TSource, TObject>(source, searchObject, properties.CopyWith(customExpressions: customExpressions));
        }
        private IQueryable<TSource> Apply(bool applyPaging = false)
        {
            if (applyPaging)
            {
                return ApplyFilters().ApplyNonFilters().ApplyPaging().source;
            }
            return ApplyFilters().ApplyNonFilters().source;
        }

    }


    internal class PropertySpecifications<TSource>
    {
        private Queue<PropertyInfo>? skipProperties;
        private Queue<PropertyInfo>? takeProperties;
        private Queue<PropertyInfo>? filterProperties;
        private Queue<PropertyInfo>? nonFilterProperties;
        private Dictionary<string, Func<IQueryable<TSource>, IQueryable<TSource>>>? customExpressions;
        private bool allowNonFilterProperties = false;
        internal bool AllowNonFilterProperties => allowNonFilterProperties;
        internal Dictionary<string, Func<IQueryable<TSource>, IQueryable<TSource>>> CustomExpressions => customExpressions ??= new Dictionary<string, Func<IQueryable<TSource>, IQueryable<TSource>>>();
        internal Queue<PropertyInfo> NonFilterProperties => nonFilterProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> FilterProperties => filterProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> SkipProperties => skipProperties ??= new Queue<PropertyInfo>();
        internal Queue<PropertyInfo> TakeProperties => takeProperties ??= new Queue<PropertyInfo>();


        internal PropertySpecifications<TSource> CopyWith(
            Queue<PropertyInfo>? skipProperties = null,
            Queue<PropertyInfo>? takeProperties = null,
            Queue<PropertyInfo>? filterProperties = null,
            Queue<PropertyInfo>? nonFilterProperties = null,
            bool? allowNonFilterProperties = null,
            Dictionary<string, Func<IQueryable<TSource>, IQueryable<TSource>>>? customExpressions = null
        )
        {
            return new PropertySpecifications<TSource>
            {
                skipProperties = skipProperties ?? this.skipProperties,
                takeProperties = takeProperties ?? this.takeProperties,
                filterProperties = filterProperties ?? this.filterProperties,
                nonFilterProperties = nonFilterProperties ?? this.nonFilterProperties,
                allowNonFilterProperties = allowNonFilterProperties ?? this.allowNonFilterProperties,
                customExpressions = customExpressions ?? this.customExpressions
            };
        }
    }

}