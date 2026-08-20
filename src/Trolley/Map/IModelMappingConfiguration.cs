namespace Trolley;

public interface IModelMappingConfiguration
{
    bool IsCanMapTo(string fromFieldName, string toMemberName);
    void OnModelCreating(ModelBuilder builder);
}