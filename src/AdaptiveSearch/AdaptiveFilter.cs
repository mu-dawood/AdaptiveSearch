

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AdaptiveSearch.Interfaces;

namespace AdaptiveSearch
{
    public enum ApplyType { And, Or }
    public class AdaptiveFilter<TSource, TFilter> : IQueryable<TSource>
     where TFilter : IAdaptiveFilter
    {
        private readonly IQueryable<TSource> _source;
        private readonly TFilter filter;
        private readonly IEnumerable<Func<ParameterExpression, Expression>> expressions;
        private readonly ApplyType type;

        private AdaptiveFilter(IQueryable<TSource> source, TFilter filter, ApplyType type, IEnumerable<Func<ParameterExpression, Expression>> expressions)
        {
            _source = source;
            this.filter = filter;
            this.expressions = expressions;
            this.type = type;
        }
        internal AdaptiveFilter(IQueryable<TSource> source, TFilter filter, ApplyType type)
        {
            _source = source;
            this.filter = filter;
            this.expressions = new Func<ParameterExpression, Expression>[] { };
            this.type = type;
        }

        public Type ElementType => _source.ElementType;

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


        public AdaptiveFilter<TSource, TFilter> ApplyTo<TProperty>(Expression<Func<TSource, TProperty>> selector)
        {
            if (!filter.HasValue) return this;
            var body = selector.Body;
            if (!(body is MemberExpression member))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.", body.ToString()));
            }
            if (!(member.Member is PropertyInfo propInfo))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.", body.ToString()));
            }
            Type sourceType = typeof(TSource);
            if (propInfo.ReflectedType != null && sourceType != propInfo.ReflectedType && !sourceType.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.", body.ToString(),
                    type));
            }
            var res = expressions.Append((p) =>
            {
                var selector = Expression.Property(p, propInfo.Name);
                return filter.BuildExpression<TSource>(selector);
            });
            return new AdaptiveFilter<TSource, TFilter>(_source, filter, type, res);
        }




        private IQueryable<TSource> Apply()
        {
            if (expressions.Count() == 0) return _source;
            var exps = new List<Expression>();
            var parameter = Expression.Parameter(typeof(TSource), "x");
            foreach (var e in expressions)
            {
                exps.Add(e(parameter));
            }
            var combined = type == ApplyType.Or ? exps.Aggregate(Expression.OrElse) : exps.Aggregate(Expression.AndAlso);
            return _source.Where(Expression.Lambda<Func<TSource, bool>>(combined, parameter));

        }



    }


}

