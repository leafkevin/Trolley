using System;
using System.Linq.Expressions;

namespace Trolley;

public class PredicateBuilder
{
    public static PredicateBuilder<T> Create<T>() => new PredicateBuilder<T>();
    public static PredicateBuilder<T1, T2> Create<T1, T2>() => new PredicateBuilder<T1, T2>();
    public static PredicateBuilder<T1, T2, T3> Create<T1, T2, T3>() => new PredicateBuilder<T1, T2, T3>();
    public static PredicateBuilder<T1, T2, T3, T4> Create<T1, T2, T3, T4>() => new PredicateBuilder<T1, T2, T3, T4>();
    public static PredicateBuilder<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>() => new PredicateBuilder<T1, T2, T3, T4, T5>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Create<T1, T2, T3, T4, T5, T6, T7, T8>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
    public static PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>() => new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
}
public class PredicateBuilder<T>
{
    private Expression<Func<T, bool>> expression;
    public PredicateBuilder<T> And(Expression<Func<T, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T> AndMerge(Func<PredicateBuilder<T>, PredicateBuilder<T>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T> Or(Expression<Func<T, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T> OrMerge(Func<PredicateBuilder<T>, PredicateBuilder<T>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T, bool>> Build() => this.expression as Expression<Func<T, bool>>;
    private PredicateBuilder<T> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2>
{
    private Expression<Func<T1, T2, bool>> expression;
    public PredicateBuilder<T1, T2> And(Expression<Func<T1, T2, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2> AndMerge(Func<PredicateBuilder<T1, T2>, PredicateBuilder<T1, T2>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2> OrMerge(Func<PredicateBuilder<T1, T2>, PredicateBuilder<T1, T2>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, bool>> Build() => this.expression as Expression<Func<T1, T2, bool>>;
    private PredicateBuilder<T1, T2> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3>
{
    private Expression<Func<T1, T2, T3, bool>> expression;
    public PredicateBuilder<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3> AndMerge(Func<PredicateBuilder<T1, T2, T3>, PredicateBuilder<T1, T2, T3>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3> OrMerge(Func<PredicateBuilder<T1, T2, T3>, PredicateBuilder<T1, T2, T3>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, bool>>;
    private PredicateBuilder<T1, T2, T3> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4>
{
    private Expression<Func<T1, T2, T3, T4, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4>, PredicateBuilder<T1, T2, T3, T4>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4>, PredicateBuilder<T1, T2, T3, T4>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, bool>>;
    private PredicateBuilder<T1, T2, T3, T4> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5>
{
    private Expression<Func<T1, T2, T3, T4, T5, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5>, PredicateBuilder<T1, T2, T3, T4, T5>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5>, PredicateBuilder<T1, T2, T3, T4, T5>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, PredicateBuilder<T1, T2, T3, T4, T5, T6>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, PredicateBuilder<T1, T2, T3, T4, T5, T6>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}
public class PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    private Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> expression;
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate) => this.Merge(Expression.AndAlso, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.AndAlso, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> AndMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>());
        return this.Merge(Expression.AndAlso, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate) => this.Merge(Expression.OrElse, predicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
        => this.Merge(condition, Expression.OrElse, ifPredicate, elsePredicate);
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrMerge(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> mergePredicate)
    {
        if (mergePredicate == null) throw new ArgumentNullException(nameof(mergePredicate));
        var builder = mergePredicate.Invoke(new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>());
        return this.Merge(Expression.OrElse, builder.Build());
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Not()
    {
        if (this.expression == null) throw new NotSupportedException("当前表达式为null，不支持Not操作");
        this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>(Expression.Not(this.expression.Body), this.expression.Parameters);
        return this;
    }
    public PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Not(bool condition)
    {
        if (condition) this.Not();
        return this;
    }
    public Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> Build() => this.expression as Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>;
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Merge(Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (this.expression == null) this.expression = predicate;
        else
        {
            var body = mergeOp.Invoke(this.expression.Body, predicate.Body);
            this.expression = Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>(body, this.expression.Parameters);
        }
        return this;
    }
    private PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Merge(bool condition, Func<Expression, Expression, Expression> mergeOp, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate)
    {
        if (condition) this.Merge(mergeOp, ifPredicate);
        else if (elsePredicate != null) this.Merge(mergeOp, elsePredicate);
        return this;
    }
}