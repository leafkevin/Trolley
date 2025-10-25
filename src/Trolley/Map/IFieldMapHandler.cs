namespace Trolley;

public interface IFieldMapHandler
{
    bool IsCanMap(string fromFieldName, string toMemberName);
}