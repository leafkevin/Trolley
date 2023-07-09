using System;
using System.Linq.Expressions;
using System.Text;

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
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Date":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Date);

                        return visitor.Change(targetSegment, $"CONVERT({this.GetQuotedValue(targetSegment)},DATE)", false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Day);

                        return visitor.Change(targetSegment, $"DAYOFMONTH({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfWeek);

                        return visitor.Change(targetSegment, $"DAYOFWEEK({this.GetQuotedValue(targetSegment)})-1", true, false);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfYear);

                        return visitor.Change(targetSegment, $"DAYOFYEAR({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Hour);

                        return visitor.Change(targetSegment, $"HOUR({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Kind);

                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Millisecond);

                        return visitor.Change(targetSegment, $"FLOOR(MICROSECOND({this.GetQuotedValue(targetSegment)})/1000)", false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Minute);

                        return visitor.Change(targetSegment, $"MINUTE({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Month);

                        return visitor.Change(targetSegment, $"MONTH({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Second);

                        return visitor.Change(targetSegment, $"SECOND({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Ticks);

                        return visitor.Change(targetSegment, $"TIMESTAMPDIFF(MICROSECOND,'0001-01-01',{this.GetQuotedValue(targetSegment)})*10", true, false);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).TimeOfDay);

                        return visitor.Change(targetSegment, $"CONVERT({this.GetQuotedValue(targetSegment)},TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Year);

                        return visitor.Change(targetSegment, $"YEAR({this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
            }
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if ((leftSegment.IsConstant || leftSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(leftSegment, rightSegment, DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(leftSegment, rightSegment, $"DAYOFMONTH(LAST_DAY(CONCAT({leftArgument},'-',{rightArgument},'-01')))", false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"(({valueArgument})%4=0 AND ({valueArgument})%100<>0 OR ({valueArgument})%400=0)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, DateTime.Parse(valueSegment.ToString()));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST({valueArgument} AS DATETIME)", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var providerSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable)
                            && (providerSegment.IsConstant || providerSegment.IsVariable))
                            return visitor.Merge(valueSegment, formatSegment, DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment}'";

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
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Merge(valueSegment, formatSegment, $"STR_TO_DATE({valueArgument},'{formatArgument}')", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(leftSegment.Clone(args[1]));

                        var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        if (rightSegment.IsConstant || rightSegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            var timeSpan = (TimeSpan)rightSegment.Value;
                            if (timeSpan.Days > 0)
                            {
                                builder.Append($"DATE_ADD({targetArgument},INTERVAL {timeSpan.Days} DAY)");
                                timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                            }
                            if (timeSpan.Ticks > 0)
                            {
                                if (builder.Length > 0) builder.Insert(0, $"ADDTIME(");
                                else builder.Append($"ADDTIME({targetArgument}");
                                builder.Append($",{this.GetQuotedValue(timeSpan)})");
                            }
                            return visitor.Merge(targetSegment, rightSegment, builder.ToString(), false, true);
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"ADDTIME({targetArgument},{rightArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                     {
                         var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                         var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                         if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                             return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                         var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                         var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                         return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} DAY)", false, true);
                     });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                           && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} HOUR)", false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument}*1000 MICROSECOND)", false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} MINUTE)", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                      {
                          var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                          var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                          if ((targetSegment.IsConstant || targetSegment.IsVariable)
                              && (rightSegment.IsConstant || rightSegment.IsVariable))
                              return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                          var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                          var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                          return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightSegment} MONTH)", false, true);
                      });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} SECOND)", false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument}/10 MICROSECOND)", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} YEAR)", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"TIMEDIFF({targetArgument},{rightArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            if (rightSegment.IsConstant || rightSegment.IsVariable)
                            {
                                var builder = new StringBuilder();
                                var timeSpan = (TimeSpan)rightSegment.Value;
                                if (timeSpan.Days > 0)
                                {
                                    builder.Append($"DATE_SUB({targetArgument},INTERVAL {timeSpan.Days} DAY)");
                                    timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                                }
                                if (timeSpan.Ticks > 0)
                                {
                                    if (builder.Length > 0) builder.Insert(0, $"SUBTIME(");
                                    else builder.Append($"SUBTIME({targetArgument}");
                                    builder.Append($",{this.GetQuotedValue(timeSpan)})");
                                }
                                return visitor.Merge(targetSegment, rightSegment, builder.ToString(), false, true);
                            }
                            //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            return visitor.Merge(targetSegment, rightSegment, $"SUBTIME({targetArgument},{rightArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, visitor.Change(targetSegment).ToString());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"DATE_FORMAT({this.GetQuotedValue(targetSegment)},'%Y-%m-%d %H:%i:%s')", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            string formatArgument = null;
                            if (rightSegment.IsConstant || rightSegment.IsVariable)
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

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"DATE_FORMAT({targetArgument},{formatArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
