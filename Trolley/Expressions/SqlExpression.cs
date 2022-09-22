//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Thea.Orm;

//namespace Trolley;

//public class SqlExpression
//{
//    private readonly IOrmDbFactory dbFactory = null;
//    private readonly IOrmProvider ormProvider = null;
//    private readonly StringBuilder sqlBuilder = new();
//    private Dictionary<Type, string> tableAlaises = null;
//    public SqlExpression(IOrmDbFactory dbFactory, IOrmProvider ormProvider)
//    {
//        this.dbFactory = dbFactory;
//        this.ormProvider = ormProvider;
//    }
//    private SqlExpressionScope currentScope = null;

//    public virtual SqlExpression<T> From<T>()
//    {
//        return null;
//    }
//    public virtual SqlExpression From<T1, T2>()
//    {
//        return this;
//    }
//    public virtual SqlExpression From<T1, T2, T3>()
//    {
//        return this;
//    }
//    public virtual SqlExpression From<T1, T2, T3, T4>()
//    {
//        return this;
//    }
//    public virtual SqlExpression From<T1, T2, T3, T4, T5>()
//    {
//        return this;
//    }
//    public virtual SqlExpression InnerJoin<T1, T2>(Expression<Func<T1, T2, bool>> onPredicate)
//    {
//        return this;
//    }
//    public virtual SqlExpression LeftJoin<T1, T2>(Expression<Func<T1, T2, bool>> onPredicate)
//    {
//        return this;
//    }
//    public virtual SqlExpression RightJoin<T1, T2>(Expression<Func<T1, T2, bool>> onPredicate)
//    {
//        return this;
//    }
//    public virtual SqlExpression Where<T1, T2>(Expression<Func<T1, T2, bool>> onPredicate)
//    {
//        return this;
//    }
//}

 