namespace Trolley;

public interface IModelMappingConfiguration
{
    bool IsCanMapTo(string fromFieldName, string toMemberName);
    //OnConfiguring
    void Configure(ModelBuilder builder);
}