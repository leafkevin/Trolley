using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

public interface IFieldMapHandler
{
    bool TryFindMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo);
    bool TryFindField(string memberName, List<string> fieldNames, out string fieldName);
}
