using System;
using System.Linq.Expressions;

namespace Trolley;

public interface IOrderBySqlExpression<T>
{
    IOrderBySqlExpression<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IOrderBySqlExpression<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
}