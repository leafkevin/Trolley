using System;
using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

public class DefaultFieldMapHandler : IFieldMapHandler
{
    public bool IsCanMap(string fromName, string toName)
    {
        if (string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(toName))
            return false;
        if (fromName == toName)
            return true;
        if (fromName.Equals(toName, StringComparison.OrdinalIgnoreCase))
            return true;
        fromName = fromName.Replace("_", string.Empty);
        if (fromName == toName)
            return true;
        if (fromName.Equals(toName, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
    public bool IsCanMap(MemberInfo fromName, MemberInfo toName)
    {
        if (fromName == null || toName == null)
            return false;
        return this.IsCanMap(fromName.Name, toName.Name);
    }
    public bool TryFindMember(string fieldName, List<MemberMap> memberMappers, out MemberMap memberMapper)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));
        memberMapper = memberMappers.Find(f => this.IsCanMap(fieldName, f.FieldName));
        if (memberMapper != null) return true;
        return false;
    }
    public bool TryFindMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));
        memberInfo = memberInfos.Find(f => this.IsCanMap(fieldName, f.Name));
        if (memberInfo != null) return true;
        return false;
    }
}