using System.Linq.Expressions;

namespace AdaptiveSearch.Interfaces
{

    public interface IAdaptiveFilter
    {
        Expression BuildExpression<TSource>(Expression property);

        bool HasValue();
    }
}