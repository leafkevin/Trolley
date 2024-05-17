namespace Trolley;

public class ShardingTableInfo
{
    public ShardingTableType ShardingType { get; set; }
    public TableSegment MasterTable { get; set; }
    public TableSegment TableSegment { get; set; }
    public object TableNameGetter { get; set; }
}
public enum ShardingTableType : byte
{
    MasterPredicate,
    SubordinateGetter
}