using System;

namespace Trolley;

public abstract class DefaultModelMappingConfiguration : IModelMappingConfiguration
{
    public virtual bool IsCanMapTo(string fromFieldName, string toMemberName)
    {
        if (string.IsNullOrEmpty(fromFieldName) || string.IsNullOrEmpty(toMemberName))
            return false;
        if (fromFieldName.Equals(toMemberName, StringComparison.OrdinalIgnoreCase))
            return true;
        fromFieldName = fromFieldName.Replace("_", string.Empty);
        if (fromFieldName.Equals(toMemberName, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
    public abstract void Configure(ModelBuilder builder);
}