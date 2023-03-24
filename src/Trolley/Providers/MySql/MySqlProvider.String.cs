using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

partial class MySqlProvider
{
    protected bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        switch (methodInfo.Name)
        {
            case "Contains":
                //String
                //public bool Contains(char value);
                //public bool Contains(char value, StringComparison comparisonType);
                //public bool Contains(String value);
                //public bool Contains(String value, StringComparison comparisonType);
                if (!methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = $"'%{rightSegment}%'";
                        else rightArgument = $"CONCAT('%',{rightSegment},'%')";

                        int notIndex = 0;
                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }
                        string notString = notIndex % 2 > 0 ? " NOT" : "";
                        return targetSegment.Change($"{targetArgument}{notString} LIKE {rightArgument}", false, true);
                    });
                    result = true;
                }
                break;
            case "Concat":
                //public static String Concat(IEnumerable<String?> values);
                //public static String Concat(params String?[] values);
                //public static String Concat<T>(IEnumerable<T> values);
                //public static String Concat(params object?[] args);
                //public static String Concat(object? arg0);
                //public static String Concat(object? arg0, object? arg1, object? arg2);
                //public static String Concat(String? str0, String? str1, String? str2, String? str3);
                //public static String Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3);
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var constBuilder = new StringBuilder();

                        foreach (var argumentSegment in args)
                        {
                            //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                            var sqlSegment = visitor.VisitAndDeferred(argumentSegment);
                            if (sqlSegment.Value is List<SqlSegment> sqlSegments)
                            {
                                foreach (var element in sqlSegments)
                                {
                                    var eleSegment = visitor.VisitAndDeferred(element);
                                    //为null时什么也不做，相当于加空字符串""
                                    if (eleSegment == SqlSegment.Null)
                                        continue;

                                    var strValue = eleSegment.ToString();
                                    if (eleSegment.IsConstantValue)
                                        constBuilder.Append(strValue);
                                    else
                                    {
                                        if (constBuilder.Length > 0)
                                        {
                                            if (builder.Length > 0)
                                                builder.Append(',');
                                            var constValue = this.GetQuotedValue(constBuilder.ToString());
                                            builder.Append(constValue);
                                            constBuilder.Clear();
                                        }
                                        if (builder.Length > 0)
                                            builder.Append(',');
                                        builder.Append(strValue);
                                    }
                                }
                            }
                            else
                            {
                                var strValue = sqlSegment.ToString();
                                if (sqlSegment.IsConstantValue)
                                    constBuilder.Append(strValue);
                                else
                                {
                                    if (constBuilder.Length > 0)
                                    {
                                        if (builder.Length > 0)
                                            builder.Append(',');
                                        var constValue = this.GetQuotedValue(constBuilder.ToString());
                                        builder.Append(constValue);
                                        constBuilder.Clear();
                                    }
                                    if (builder.Length > 0)
                                        builder.Append(',');
                                    builder.Append(strValue);
                                }
                            }
                        }
                        if (builder.Length > 0)
                        {
                            if (constBuilder.Length > 0)
                            {
                                builder.Append(',');
                                var constValue = this.GetQuotedValue(constBuilder.ToString());
                                builder.Append(constValue);
                                constBuilder.Clear();
                            }
                            builder.Insert(0, "CONCAT(");
                            builder.Append(')');
                            return args[0].Change(builder.ToString(), false, true);
                        }
                        else return args[0].Change(constBuilder.ToString());
                    });
                    result = true;
                }
                break;
            case "Format":
                //public static String Format(String format, object? arg0);
                //public static String Format(String format, object? arg0, object? arg1); 
                //public static String Format(String format, object? arg0, object? arg1, object? arg2); 
                //public static String Format(String format, params object?[] args);
                if (methodInfo.IsStatic && parameterInfos.Length >= 2)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var constBuilder = new StringBuilder();

                        var concatSegments = visitor.ConvertFormatToConcatList(args);


                        var fmtResult = visitor.Evaluate<string>(args[0].Expression);

                        int lastIndex = 0;
                        return target;
                        //for (int i = 1; i < args.Length; i++)
                        //{
                        //    //123_{0}_345_{1}{2}_etr_{3}_fdr, 111,@p1,@p2,e4re
                        //    //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                        //    //var sqlSegment = visitor.VisitAndDeferred(args[i]);
                        //    var sqlSegment = visitor.VisitConcatAndDeferred(args[i]);
                        //    if (sqlSegment.Value is List<SqlSegment> sqlSegments)
                        //    {
                        //        foreach (var eleSegment in sqlSegments)
                        //        {
                        //            //为null时什么也不做，相当于加空字符串""
                        //            if (eleSegment == SqlSegment.Null)
                        //                continue;

                        //            //把参数之间的Format常量添加到constBuilder中，同时指针后移到下一个参数起始位置
                        //            var index = fmtResult.IndexOf("{", lastIndex);
                        //            if (index > lastIndex)
                        //                constBuilder.Append(fmtResult.Substring(lastIndex, index - lastIndex));

                        //            var strValue = eleSegment.ToString();
                        //            if (eleSegment.IsConstantValue)
                        //                constBuilder.Append(strValue);
                        //            else
                        //            {
                        //                if (constBuilder.Length > 0)
                        //                {
                        //                    if (builder.Length > 0)
                        //                        builder.Append(',');
                        //                    var constValue = this.GetQuotedValue(constBuilder.ToString());
                        //                    builder.Append(constValue);
                        //                    constBuilder.Clear();
                        //                }
                        //                if (builder.Length > 0)
                        //                    builder.Append(',');
                        //                builder.Append(strValue);
                        //            }
                        //            lastIndex = fmtResult.IndexOf('}', index + 2) + 1;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //把参数之间的Format常量添加到constBuilder中，同时指针后移到下一个参数起始位置
                        //        var index = fmtResult.IndexOf("{", lastIndex);
                        //        if (index > lastIndex)
                        //            constBuilder.Append(fmtResult.Substring(lastIndex, index - lastIndex));

                        //        var strValue = sqlSegment.ToString();
                        //        if (sqlSegment.IsConstantValue)
                        //            constBuilder.Append(strValue);
                        //        else
                        //        {
                        //            if (constBuilder.Length > 0)
                        //            {
                        //                if (builder.Length > 0)
                        //                    builder.Append(',');
                        //                var constValue = this.GetQuotedValue(constBuilder.ToString());
                        //                builder.Append(constValue);
                        //                constBuilder.Clear();
                        //            }
                        //            if (builder.Length > 0)
                        //                builder.Append(',');
                        //            builder.Append(strValue);
                        //        }
                        //        lastIndex = fmtResult.IndexOf('}', index + 2) + 1;
                        //    }
                        //}
                        //if (lastIndex < fmtResult.Length)
                        //    constBuilder.Append(fmtResult.Substring(lastIndex));
                        //if (builder.Length > 0)
                        //{
                        //    if (constBuilder.Length > 0)
                        //    {
                        //        builder.Append(',');
                        //        var constValue = this.GetQuotedValue(constBuilder.ToString());
                        //        builder.Append(constValue);
                        //        constBuilder.Clear();
                        //    }
                        //    builder.Insert(0, "CONCAT(");
                        //    builder.Append(')');
                        //    return args[0].Change(builder.ToString(), false, true);
                        //}
                        //else return args[0].Change(constBuilder.ToString());
                    });
                    result = true;
                }
                break;
            case "Compare":
            case "CompareOrdinal":
                //String.Compare  不区分大小写
                //public static int Compare(String? strA, String? strB);
                //public static int Compare(String? strA, String? strB, bool ignoreCase);
                //public static int Compare(String? strA, String? strB, bool ignoreCase, CultureInfo? culture);
                if (methodInfo.IsStatic && parameterInfos.Length >= 2 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegument = visitor.VisitAndDeferred(args[1]);

                        string leftArgument = null;
                        if (leftSegment.IsConstantValue)
                            leftArgument = this.GetQuotedValue(leftSegment.ToString());
                        else leftArgument = leftSegment.ToString();

                        string rightArgument = null;
                        if (rightSegument.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegument.ToString());
                        else rightArgument = rightSegument.ToString();

                        return leftSegment.Change($"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                }
                break;
            case "CompareTo":
                //各种类型都有CompareTo方法
                //public int CompareTo(Boolean value);
                //public int CompareTo(Int32 value);
                //public int CompareTo(Double value);
                //public int CompareTo(DateTime value);
                //public int CompareTo(object? value);
                methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    var rightSegument = visitor.VisitAndDeferred(args[0]);

                    string targetArgument = null;
                    if (targetSegment.IsConstantValue)
                        targetArgument = this.GetQuotedValue(targetSegment.ToString());
                    else targetArgument = targetSegment.ToString();

                    string rightArgument = null;
                    if (rightSegument.IsConstantValue)
                        rightArgument = this.GetQuotedValue(rightSegument.ToString());
                    else rightArgument = rightSegument.ToString();

                    return targetSegment.Change($"(CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END)", false, true);
                });
                result = true;
                break;
            case "Trim":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        return targetSegment.Change($"TRIM({targetArgument})", false, true);
                    };
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                }
                break;
            case "TrimStart":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        return targetSegment.Change($"LTRIM({targetArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "TrimEnd":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        return targetSegment.Change($"RTRIM({targetArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "ToUpper":
                if (!methodInfo.IsStatic && parameterInfos.Length >= 0 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        return targetSegment.Change($"UPPER({targetArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "ToLower":
                if (!methodInfo.IsStatic && parameterInfos.Length >= 0 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        return targetSegment.Change($"LOWER({targetArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "Equals":
                //各种类型都有Equals方法
                //public bool Equals(Boolean value);
                //public bool Equals(Int32 value);
                //public bool Equals(Double value);
                //public bool Equals(DateTime value);
                //public bool Equals(object? value);
                methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    var rightSegment = visitor.VisitAndDeferred(args[0]);

                    string targetArgument = null;
                    if (targetSegment.IsConstantValue)
                        targetArgument = this.GetQuotedValue(targetSegment.ToString());
                    else targetArgument = targetSegment.ToString();

                    string rightArgument = null;
                    if (rightSegment.IsConstantValue)
                        rightArgument = this.GetQuotedValue(rightSegment.ToString());
                    else rightArgument = rightSegment.ToString();

                    int notIndex = 0;
                    if (deferExprs != null && deferExprs.Count > 0)
                    {
                        while (deferExprs.TryPop(out var deferredExpr))
                        {
                            switch (deferredExpr.OperationType)
                            {
                                case OperationType.Equal:
                                    continue;
                                case OperationType.Not:
                                    notIndex++;
                                    break;
                            }
                        }
                    }
                    string equalsString = notIndex % 2 > 0 ? "<>" : "=";
                    return targetSegment.Change($"{targetArgument}{equalsString}{rightSegment}", false, true);
                });
                result = true;
                break;
            case "StartsWith":
                if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue($"{rightSegment}%");
                        else rightArgument = $"CONCAT({rightSegment},'%')";
                        int notIndex = 0;

                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }
                        string notString = notIndex % 2 > 0 ? " NOT" : "";
                        return targetSegment.Change($"{targetArgument}{notString} LIKE {rightArgument}", false, true);
                    });
                    result = true;
                }
                break;
            case "EndsWith":
                if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue($"%{rightSegment}");
                        else rightArgument = $"CONCAT('%',{rightSegment})";
                        int notIndex = 0;

                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }
                        string notString = notIndex % 2 > 0 ? " NOT" : "";
                        return targetSegment.Change($"{targetArgument}{notString} LIKE {rightArgument}", false, true);
                    });
                    result = true;
                }
                break;
            case "Substring":
                if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    if (parameterInfos.Length > 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var indexSegment = visitor.VisitAndDeferred(args[0]);
                            var lengthSegment = visitor.VisitAndDeferred(args[1]);

                            if (targetSegment.IsConstantValue && indexSegment.IsConstantValue && lengthSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value), Convert.ToInt32(lengthSegment.Value)));

                            string targetArgument = null;
                            if (targetSegment.IsConstantValue)
                                targetArgument = this.GetQuotedValue(targetSegment.ToString());
                            else targetArgument = targetSegment.ToString();
                            return targetSegment.Change($"SUBSTR({targetArgument},{indexSegment}+1,{lengthSegment})", false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var indexSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && indexSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value)));

                            string targetArgument = null;
                            if (targetSegment.IsConstantValue)
                                targetArgument = this.GetQuotedValue(targetSegment.ToString());
                            else targetArgument = targetSegment.ToString();

                            return targetSegment.Change($"SUBSTR({targetArgument},{indexSegment}+1)");
                        });
                        result = true;
                    }
                }
                break;
            case "ToString":
                //if (methodInfo.IsStatic && parameterInfos.Length >= 1 && methodInfo.DeclaringType == typeof(Convert))
                //{
                //    //Convert.ToString(bool)
                //    //Convert.ToString(int)
                //    //Convert.ToString(double)
                //    //Convert.ToString(DateTime)                  
                //    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                //    {
                //        var rightSegment = visitor.VisitAndDeferred(args[0]);
                //        if (rightSegment.IsConstantValue)
                //            return rightSegment.Change(rightSegment.ToString());

                //        return rightSegment.Change($"CAST({rightSegment} AS {this.CastTo(typeof(string))})", false, true);
                //    });
                //    result = true;
                //}
                if (!methodInfo.IsStatic && parameterInfos.Length >= 0)
                {
                    //int.ToString();
                    //int.ToString(IFormatProvider);
                    //double.ToString();
                    //double.ToString(IFormatProvider);
                    //DateTime.ToString();
                    if (parameterInfos.Length == 0 || (parameterInfos.Length == 1 && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.ToString());
                            return targetSegment.Change($"CAST({targetSegment} AS {this.CastTo(typeof(string))})", false, true);
                        });
                        result = true;
                    }
                    //放到其他类型的方法中实现
                    //int.ToString(string format);
                    //double.ToString(string format);
                    //DateTime.ToString(string format);
                }
                break;
            case "IsNullOrEmpty":
                if (methodInfo.IsStatic && parameterInfos.Length == 1 && typeof(string).IsAssignableTo(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(args[0]);
                        string targetAugment = null;
                        if (targetSegment.IsConstantValue)
                            targetAugment = this.GetQuotedValue(targetSegment.ToString());
                        else targetAugment = targetSegment.ToString();
                        return targetSegment.Change($"({targetAugment} IS NULL OR {targetAugment}='')", false, true);
                    });
                    result = true;
                }
                break;
            case "IsNullOrWhiteSpace":
                if (methodInfo.IsStatic && parameterInfos.Length == 1 && typeof(string).IsAssignableTo(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(args[0]);
                        string targetAugment = null;
                        if (targetSegment.IsConstantValue)
                            targetAugment = this.GetQuotedValue(targetSegment.ToString());
                        else targetAugment = targetSegment.ToString();
                        return targetSegment.Change($"({targetAugment} IS NULL OR {targetAugment}='' OR TRIM({targetAugment})='')", false, true);
                    });
                    result = true;
                }
                break;
            case "Join":
                if (methodInfo.IsStatic && parameterInfos.Length == 2 && typeof(string).IsAssignableTo(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var separatorSegment = visitor.VisitAndDeferred(args[0]);
                        var valuesSegment = visitor.VisitAndDeferred(args[1]);

                        if (separatorSegment.IsConstantValue && valuesSegment.IsConstantValue)
                            return valuesSegment.Change(string.Join(separatorSegment.ToString(), valuesSegment.Value as List<SqlSegment>));

                        string separatorAugment = null;
                        if (separatorSegment.IsConstantValue)
                            separatorAugment = this.GetQuotedValue(separatorSegment.ToString());
                        else separatorAugment = separatorSegment.ToString();

                        var sqlSegments = valuesSegment.Value as List<SqlSegment>;
                        var builder = new StringBuilder();
                        var constBuilder = new StringBuilder();
                        foreach (var sqlSegment in sqlSegments)
                        {
                            var strValue = sqlSegment.ToString();

                            if (separatorSegment.IsConstantValue && constBuilder.Length > 0)
                                constBuilder.Append(separatorAugment);
                            if (!separatorSegment.IsConstantValue && builder.Length > 0)
                                builder.Append(separatorAugment);

                            if (sqlSegment.IsConstantValue)
                            {
                                if (separatorSegment.IsConstantValue && constBuilder.Length > 0)
                                    constBuilder.Append(separatorAugment);
                                constBuilder.Append(strValue);
                            }
                            else
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                if (constBuilder.Length > 0)
                                {
                                    var constValue = this.GetQuotedValue(constBuilder.ToString());
                                    builder.Append(constValue);
                                    constBuilder.Clear();
                                }
                                builder.Append(',');
                                builder.Append(strValue);
                            }
                        }
                        if (builder.Length > 0)
                        {
                            if (constBuilder.Length > 0)
                            {
                                builder.Append(',');
                                var constValue = this.GetQuotedValue(constBuilder.ToString());
                                builder.Append(constValue);
                                constBuilder.Clear();
                            }
                            builder.Insert(0, "CONCAT(");
                            builder.Append(')');
                            return valuesSegment.Change(builder.ToString(), false, true);
                        }
                        else return valuesSegment.Change(constBuilder.ToString());
                    });
                    result = true;
                }
                if (methodInfo.IsStatic && parameterInfos.Length > 2 && typeof(string).IsAssignableTo(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var separatorSegment = visitor.VisitAndDeferred(args[0]);
                        var valuesSegment = visitor.VisitAndDeferred(args[1]);
                        var index = visitor.Evaluate<int>(args[2].Expression);
                        var length = visitor.Evaluate<int>(args[3].Expression);

                        if (separatorSegment.IsConstantValue && valuesSegment.IsConstantValue)
                            return valuesSegment.Change(string.Join(separatorSegment.ToString(), valuesSegment.Value as List<SqlSegment>, index, length));

                        string separatorAugment = null;
                        if (separatorSegment.IsConstantValue)
                            separatorAugment = this.GetQuotedValue(separatorSegment.ToString());
                        else separatorAugment = separatorSegment.ToString();

                        var sqlSegments = valuesSegment.Value as List<SqlSegment>;
                        var builder = new StringBuilder();
                        var constBuilder = new StringBuilder();
                        int count = Math.Min(sqlSegments.Count, index + length);
                        for (int i = index; i < count; i++)
                        {
                            var strValue = sqlSegments[i].ToString();

                            if (separatorSegment.IsConstantValue && constBuilder.Length > 0)
                                constBuilder.Append(separatorAugment);
                            if (!separatorSegment.IsConstantValue && builder.Length > 0)
                                builder.Append(separatorAugment);

                            if (sqlSegments[i].IsConstantValue)
                            {
                                if (separatorSegment.IsConstantValue && constBuilder.Length > 0)
                                    constBuilder.Append(separatorAugment);
                                constBuilder.Append(strValue);
                            }
                            else
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                if (constBuilder.Length > 0)
                                {
                                    var constValue = this.GetQuotedValue(constBuilder.ToString());
                                    builder.Append(constValue);
                                    constBuilder.Clear();
                                }
                                builder.Append(',');
                                builder.Append(strValue);
                            }
                        }
                        if (builder.Length > 0)
                        {
                            if (constBuilder.Length > 0)
                            {
                                builder.Append(',');
                                var constValue = this.GetQuotedValue(constBuilder.ToString());
                                builder.Append(constValue);
                                constBuilder.Clear();
                            }
                            builder.Insert(0, "CONCAT(");
                            builder.Append(')');
                            return valuesSegment.Change(builder.ToString(), false, true);
                        }
                        else return valuesSegment.Change(constBuilder.ToString());
                    });
                    result = true;
                }
                break;
            case "PadLeft":
                break;
        }
        return result;
    }
}
