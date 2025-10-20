using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

public interface IFieldMapHandler
{
    bool IsCanMap(string fromName, string toName);
    bool IsCanMap(MemberInfo fromName, MemberInfo toName);
    bool TryFindMember(string fieldName, List<MemberMap> memberMappers, out MemberMap memberMapper);
    bool TryFindMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo);
}