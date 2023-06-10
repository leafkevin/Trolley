using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                case "MinValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MaxValue));
                    result = true;
                    break;
                case "UnixEpoch":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.UnixEpoch));
                    result = true;
                    break;
                case "Today":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("CURDATE()", false, false, true));
                    result = true;
                    break;
                case "Now":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("NOW()", false, false, true));
                    result = true;
                    break;
                case "UtcNow":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("UTC_TIMESTAMP()", false, false, true));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Date":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Date);

                    return targetSegment.Change($"CONVERT({this.GetQuotedValue(targetSegment)},DATE)", false, false, true);
                });
                result = true;
                break;
            case "Day":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                    return targetSegment.Change($"DAYOFMONTH({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "DayOfWeek":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                    return targetSegment.Change($"DAYOFWEEK({this.GetQuotedValue(targetSegment)})-1", false, true, false);
                });
                result = true;
                break;
            case "DayOfYear":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                    return targetSegment.Change($"DAYOFYEAR({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Hour":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                    return targetSegment.Change($"HOUR({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Kind":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Kind);
                    throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                });
                result = true;
                break;
            case "Millisecond":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                    return targetSegment.Change($"FLOOR(MICROSECOND({this.GetQuotedValue(targetSegment)})/1000)", false, false, true);
                });
                result = true;
                break;
            case "Minute":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                    return targetSegment.Change($"MINUTE({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Month":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                    return targetSegment.Change($"MONTH({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Second":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                    return targetSegment.Change($"SECOND({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Ticks":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                    return targetSegment.Change($"TIMESTAMPDIFF(MICROSECOND,'0001-01-01',{this.GetQuotedValue(targetSegment)})*10", false, true, false);
                });
                result = true;
                break;
            case "TimeOfDay":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                    return targetSegment.Change($"TIMESTAMPDIFF(MICROSECOND,CONVERT({this.GetQuotedValue(targetSegment)},DATE),{this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Year":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                    return targetSegment.Change($"YEAR({this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public override bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                case "DaysInMonth":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        if (leftSegment.IsConstant && rightSegment.IsConstant)
                            return leftSegment.Change(DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"DAYOFMONTH(LAST_DAY(CONCAT({leftSegment},'-',{rightSegment},'-01')))", false, false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        return args[0].Change($"(({valueSegment})%4=0 AND ({valueSegment})%100<>0 OR ({valueSegment})%400=0)", false, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            return valueSegment.Change(DateTime.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        var formatSegment = visitor.VisitAndDeferred(args[1]);
                        var providerSegment = visitor.VisitAndDeferred(args[2]);

                        if (valueSegment.IsConstant && formatSegment.IsConstant && providerSegment.IsConstant)
                            return valueSegment.Change(DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment}'";

                            //分钟
                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.NextReplace("mm", "%i");
                            else formatArgument = formatArgument.NextReplace("m", "%i");

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.NextReplace("yyyy", "%Y");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.NextReplace("yyy", "%Y");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.NextReplace("yy", "%y");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.NextReplace("MMMM", "%M");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.NextReplace("MMM", "%b");
                            else if (formatArgument.Contains("MM"))
                                formatArgument = formatArgument.NextReplace("MM", "%m");
                            else if (formatArgument.Contains("M"))
                                formatArgument = formatArgument.NextReplace("M", "%c");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.NextReplace("dddd", "%W");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.NextReplace("ddd", "%a");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.NextReplace("dd", "%d");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.NextReplace("d", "%e");

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.NextReplace("HH", "%H");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.NextReplace("H", "%k");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.NextReplace("hh", "%h");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.NextReplace("h", "%l");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.NextReplace("ss", "%s");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.NextReplace("s", "%s");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.NextReplace("tt", "%p");
                            else if (formatArgument.Contains("t"))
                                formatArgument = formatArgument.NextReplace("t", "SUBSTRING(%p,1,1)");
                        }

                        valueSegment.Merge(formatSegment);
                        return valueSegment.Change($"STR_TO_DATE({this.GetQuotedValue(valueSegment)},'{formatArgument}')", false, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"CASE WHEN {this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END", false, true, false);
                    });
                    result = true;
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
                case "Add":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({this.GetQuotedValue(rightSegment)}) MICROSECOND)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                     {
                         var targetSegment = visitor.VisitAndDeferred(target);
                         var rightSegment = visitor.VisitAndDeferred(args[0]);

                         if (targetSegment.IsConstant && rightSegment.IsConstant)
                             return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                         targetSegment.Merge(rightSegment);
                         return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) DAY)", false, false, true);
                     });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) HOUR)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment})*1000 MICROSECOND)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) MINUTE)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                      {
                          var targetSegment = visitor.VisitAndDeferred(target);
                          var rightSegment = visitor.VisitAndDeferred(args[0]);

                          if (targetSegment.IsConstant && rightSegment.IsConstant)
                              return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                          targetSegment.Merge(rightSegment);
                          return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) MONTH)", false, false, true);
                      });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) SECOND)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment})/10 MICROSECOND)", false, false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATE_ADD({this.GetQuotedValue(targetSegment)},INTERVAL({rightSegment}) YEAR)", false, false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"TIME(TIMESTAMPDIFF(MICROSECOND,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)}))", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"DATE_SUB({this.GetQuotedValue(targetSegment)},INTERVAL {this.GetQuotedValue(rightSegment)} MICROSECOND)", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true, false);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"CASE WHEN {this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN TIMESTAMPDIFF(MICROSECOND,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END", false, true, false);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstant)
                                return targetSegment.Change(this.GetQuotedValue(targetSegment));

                            return targetSegment.Change($"DATE_FORMAT({this.GetQuotedValue(targetSegment)},'%Y-%m-%d %H:%i:%s')", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            string formatArgument = null;
                            if (rightSegment.IsConstant)
                            {
                                formatArgument = $"'{rightSegment}'";

                                //分钟
                                if (formatArgument.Contains("mm"))
                                    formatArgument = formatArgument.NextReplace("mm", "%i");
                                else formatArgument = formatArgument.NextReplace("m", "%i");

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.NextReplace("yyyy", "%Y");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.NextReplace("yyy", "%Y");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.NextReplace("yy", "%y");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.NextReplace("MMMM", "%M");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.NextReplace("MMM", "%b");
                                else if (formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.NextReplace("MM", "%m");
                                else if (formatArgument.Contains("M"))
                                    formatArgument = formatArgument.NextReplace("M", "%c");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.NextReplace("dddd", "%W");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.NextReplace("ddd", "%a");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.NextReplace("dd", "%d");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.NextReplace("d", "%e");

                                if (formatArgument.Contains("HH"))
                                    formatArgument = formatArgument.NextReplace("HH", "%H");
                                else if (formatArgument.Contains("H"))
                                    formatArgument = formatArgument.NextReplace("H", "%k");
                                else if (formatArgument.Contains("hh"))
                                    formatArgument = formatArgument.NextReplace("hh", "%h");
                                else if (formatArgument.Contains("h"))
                                    formatArgument = formatArgument.NextReplace("h", "%l");

                                if (formatArgument.Contains("ss"))
                                    formatArgument = formatArgument.NextReplace("ss", "%s");
                                else if (formatArgument.Contains("s"))
                                    formatArgument = formatArgument.NextReplace("s", "%s");

                                if (formatArgument.Contains("tt"))
                                    formatArgument = formatArgument.NextReplace("tt", "%p");
                                else if (formatArgument.Contains("t"))
                                    formatArgument = formatArgument.NextReplace("t", "SUBSTRING(%p,1,1)");
                            }

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return targetSegment.Change(((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                            return targetSegment.Change($"DATE_FORMAT({this.GetQuotedValue(targetSegment)},{formatArgument})", false, false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
