using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public static class Specification
{
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate) => predicate;
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        => first.Compose(second, Expression.AndAlso);
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
    {
        if (condition) first.Compose(second, Expression.AndAlso);
        return first;
    }
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        => first.Compose(second, Expression.OrElse);
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
    {
        if (condition) first.Compose(second, Expression.OrElse);
        return first;
    }
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        => Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
    private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
        var map = first.Parameters
            .Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }
    class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;
        ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            => new ParameterRebinder(map).Visit(exp);
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (map.TryGetValue(p, out var replacement))
                p = replacement;
            return base.VisitParameter(p);
        }
    }
}
