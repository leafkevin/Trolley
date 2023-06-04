using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        bool result = false;
        formatter = null;
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "Empty":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("''"));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Length":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((string)targetSegment.Value).Length);

                    return targetSegment.Change($"CHAR_LENGTH({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public override bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "Concat":
                    //public static String Concat(IEnumerable<String?> values);
                    //public static String Concat(params String?[] values);
                    //public static String Concat<T>(IEnumerable<T> values);
                    //public static String Concat(params object?[] args);
                    //public static String Concat(object? arg0);
                    //public static String Concat(object? arg0, object? arg1, object? arg2);
                    //public static String Concat(String? str0, String? str1, String? str2, String? str3);
                    //public static String Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3);
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            var concatSegments = visitor.SplitConcatList(args);
                            SqlSegment resultSegment = null;

                            for (var i = 0; i < concatSegments.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(concatSegments[i]);
                                if (i == 0) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);

                                if (sqlSegment.IsConstantValue)
                                    constBuilder.Append(sqlSegment.ToString());
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
                                    builder.Append(sqlSegment.ToString());
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
                        });
                        result = true;
                    }
                    break;
                case "Format":
                    //public static String Format(String format, object? arg0);
                    //public static String Format(String format, object? arg0, object? arg1); 
                    //public static String Format(String format, object? arg0, object? arg1, object? arg2); 
                    //public static String Format(String format, params object?[] args);
                    if (parameterInfos.Length >= 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            //已经被分割成了多个SqlSegment
                            var concatSegments = visitor.ConvertFormatToConcatList(args);
                            SqlSegment resultSegment = null;

                            //123_{0}_345_{1}{2}_etr_{3}_fdr, 111,@p1,@p2,e4re
                            for (var i = 0; i < concatSegments.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(concatSegments[i]);
                                if (i == 0) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);

                                if (sqlSegment.IsConstantValue)
                                {
                                    constBuilder.Append(sqlSegment.ToString());
                                    continue;
                                }
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
                                builder.Append(sqlSegment.ToString());
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
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
                    if (parameterInfos.Length >= 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(args[0]);
                            var rightSegment = visitor.VisitAndDeferred(args[1]);
                            var leftArgument = this.GetQuotedValue(leftSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            leftSegment.Merge(rightSegment);
                            return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrEmpty":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(args[0]);
                            string targetAugment = null;
                            if (valueSegment.IsConstantValue)
                                targetAugment = this.GetQuotedValue(valueSegment.ToString());
                            else targetAugment = valueSegment.ToString();
                            return valueSegment.Change($"({targetAugment} IS NULL OR {targetAugment}='')", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrWhiteSpace":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(args[0]);
                            string targetAugment = null;
                            if (targetSegment.IsConstantValue)
                                targetAugment = this.GetQuotedValue(targetSegment.ToString());
                            else targetAugment = targetSegment.ToString();
                            return targetSegment.Change($"({targetAugment} IS NULL OR {targetAugment}='' OR TRIM({targetAugment})='')", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Join":
                    if (parameterInfos.Length == 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(args[0]);
                            var valuesSegment = visitor.VisitAndDeferred(args[1]);

                            var builder = new StringBuilder();
                            if (separatorSegment.IsConstantValue && valuesSegment.IsConstantValue)
                            {
                                var enumerable = valuesSegment.Value as IEnumerable;
                                foreach (var item in enumerable)
                                {
                                    if (builder.Length > 0)
                                        builder.Append(separatorSegment.ToString());
                                    builder.Append(this.GetQuotedValue(item));
                                }
                                return valuesSegment.Change(builder.ToString(), true, false, false);
                            }

                            string separatorAugment = null;
                            if (separatorSegment.IsConstantValue)
                                separatorAugment = this.GetQuotedValue(separatorSegment.ToString());
                            else separatorAugment = separatorSegment.ToString();

                            var sqlSegments = valuesSegment.Value as List<SqlSegment>;
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
                                return valuesSegment.Change(builder.ToString(), false, false, true);
                            }
                            else return valuesSegment.Change(constBuilder.ToString());
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(args[0]);
                            var valuesSegment = visitor.VisitAndDeferred(args[1]);
                            var index = visitor.Evaluate<int>(args[2].Expression);
                            var length = visitor.Evaluate<int>(args[3].Expression);

                            var builder = new StringBuilder();
                            if (separatorSegment.IsConstantValue && valuesSegment.IsConstantValue)
                            {
                                var enumerable = valuesSegment.Value as IEnumerable;
                                int i = 0;
                                foreach (var item in enumerable)
                                {
                                    if (i < index) continue;
                                    if (i >= index + length) break;
                                    if (builder.Length > 0)
                                        builder.Append(separatorSegment.ToString());
                                    builder.Append(this.GetQuotedValue(item));
                                    i++;
                                }
                                return valuesSegment.Change(builder.ToString(), true, false, false);
                            }

                            string separatorAugment = null;
                            if (separatorSegment.IsConstantValue)
                                separatorAugment = this.GetQuotedValue(separatorSegment.ToString());
                            else separatorAugment = separatorSegment.ToString();

                            var sqlSegments = valuesSegment.Value as List<SqlSegment>;

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
                                return valuesSegment.Change(builder.ToString(), false, false, true);
                            }
                            else return valuesSegment.Change(constBuilder.ToString());
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    if (parameterInfos.Length >= 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(args[0]);
                            var rightSegment = visitor.VisitAndDeferred(args[1]);

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
                            leftSegment.Merge(rightSegment);
                            return leftSegment.Change($"{this.GetQuotedValue(leftSegment)}{equalsString}{this.GetQuotedValue(rightSegment)}", false, true, false);
                        });
                        result = true;
                    }
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
                case "Contains":
                    //String
                    //public bool Contains(char value);
                    //public bool Contains(char value, StringComparison comparisonType);
                    //public bool Contains(String value);
                    //public bool Contains(String value, StringComparison comparisonType);
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            string rightArgument = null;
                            if (rightSegment.IsConstantValue)
                                rightArgument = $"'%{rightSegment}%'";
                            else rightArgument = $"CONCAT('%',REPLACE({rightSegment},'%','\\%'),'%')";

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
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}{notString} LIKE {rightArgument}", false, true, false);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegument = visitor.VisitAndDeferred(args[0]);
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegument);

                        targetSegment.Merge(rightSegument);
                        return targetSegment.Change($"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, true, false);
                    });
                    result = true;
                    break;
                case "Trim":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"TRIM({this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"TRIM(BOTH {this.GetQuotedValue(rightSegment)} FROM {this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "TrimStart":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"LTRIM({this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"TRIM(LEADING {this.GetQuotedValue(rightSegment)} FROM {this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "TrimEnd":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"RTRIM({this.GetQuotedValue(targetSegment)})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"TRIM(TRAILING {this.GetQuotedValue(rightSegment)} FROM {this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToUpper":
                    if (parameterInfos.Length >= 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"UPPER({this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToLower":
                    if (parameterInfos.Length >= 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"LOWER({this.GetQuotedValue(targetSegment)})", false, false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

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
                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}{equalsString}{this.GetQuotedValue(rightSegment)}", false, true, false);
                    });
                    result = true;
                    break;
                case "StartsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            string rightArgument = null;
                            if (rightSegment.IsConstantValue)
                                rightArgument = $"'{rightSegment}%'";
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
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}{notString} LIKE {rightArgument}", false, true);
                        });
                        result = true;
                    }
                    break;
                case "EndsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            string rightArgument = null;
                            if (rightSegment.IsConstantValue)
                                rightArgument = $"'%{rightSegment}'";
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
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}{notString} LIKE {rightArgument}", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var indexSegment = visitor.VisitAndDeferred(args[0]);
                            var lengthSegment = visitor.VisitAndDeferred(args[1]);

                            if (targetSegment.IsConstantValue && indexSegment.IsConstantValue && lengthSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value), Convert.ToInt32(lengthSegment.Value)));

                            targetSegment.Merge(indexSegment);
                            targetSegment.Merge(lengthSegment);
                            return targetSegment.Change($"SUBSTR({this.GetQuotedValue(targetSegment)},{indexSegment}+1,{lengthSegment})", false, false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var indexSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && indexSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value)));

                            targetSegment.Merge(indexSegment);
                            return targetSegment.Change($"SUBSTR({this.GetQuotedValue(targetSegment)},{indexSegment}+1)", false, false, true);
                        });
                        result = true;
                    }
                    break;

                case "ToString":
                    if (parameterInfos.Length >= 0)
                    {
                        //int.ToString();
                        //int.ToString(IFormatProvider);
                        //double.ToString();
                        //double.ToString(IFormatProvider);
                        //DateTime.ToString();
                        if (parameterInfos.Length == 0 || (parameterInfos.Length == 1 && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                        {
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                            {
                                var targetSegment = visitor.VisitAndDeferred(target);
                                if (targetSegment.IsConstantValue)
                                    return targetSegment.Change(this.GetQuotedValue(targetSegment));
                                return targetSegment.Change(this.CastTo(typeof(string), this.GetQuotedValue(targetSegment)), false, false, true);
                            });
                            result = true;
                        }
                        //放到其他类型的方法中实现
                        //int.ToString(string format);
                        //double.ToString(string format);
                        //DateTime.ToString(string format);
                    }
                    break;
                case "IndexOf":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var valueSegment = visitor.VisitAndDeferred(args[0]);
                            targetSegment.Merge(valueSegment);
                            return targetSegment.Change($"LOCATE({this.GetQuotedValue(valueSegment)},{this.GetQuotedValue(targetSegment)})-1", false, true, false);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var valueSegment = visitor.VisitAndDeferred(args[0]);
                            var startIndexSegment = visitor.VisitAndDeferred(args[1]);
                            string startIndex = null;
                            if (startIndexSegment.IsConstantValue)
                                startIndex = $"{(int)startIndexSegment.Value + 1}";
                            else startIndex = $"{startIndexSegment}+1";
                            targetSegment.Merge(valueSegment);
                            targetSegment.Merge(startIndexSegment);
                            return targetSegment.Change($"LOCATE({this.GetQuotedValue(valueSegment)},{this.GetQuotedValue(targetSegment)},{startIndex})-1", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "PadLeft":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var widthSegment = visitor.VisitAndDeferred(args[0]);
                            targetSegment.Merge(widthSegment);
                            return targetSegment.Change($"LPAD({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(widthSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var widthSegment = visitor.VisitAndDeferred(args[0]);
                            var paddingSegment = visitor.VisitAndDeferred(args[1]);
                            targetSegment.Merge(widthSegment);
                            targetSegment.Merge(paddingSegment);
                            return targetSegment.Change($"LPAD({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(widthSegment)},{this.GetQuotedValue(paddingSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "PadRight":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var widthSegment = visitor.VisitAndDeferred(args[0]);
                            targetSegment.Merge(widthSegment);
                            return targetSegment.Change($"RPAD({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(widthSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var widthSegment = visitor.VisitAndDeferred(args[0]);
                            var paddingSegment = visitor.VisitAndDeferred(args[1]);
                            targetSegment.Merge(widthSegment);
                            targetSegment.Merge(paddingSegment);
                            return targetSegment.Change($"RPAD({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(widthSegment)},{this.GetQuotedValue(paddingSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Replace":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var oldSegment = visitor.VisitAndDeferred(args[0]);
                        var newSegment = visitor.VisitAndDeferred(args[1]);
                        targetSegment.Merge(oldSegment);
                        targetSegment.Merge(newSegment);
                        return targetSegment.Change($"REPLACE({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(oldSegment)},{this.GetQuotedValue(newSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Insert":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var indexSegment = visitor.VisitAndDeferred(args[0]);
                        var valueSegment = visitor.VisitAndDeferred(args[1]);
                        targetSegment.Merge(indexSegment);
                        targetSegment.Merge(valueSegment);
                        return targetSegment.Change($"INSERT({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(indexSegment)},0,{this.GetQuotedValue(valueSegment)})", false, false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
}
