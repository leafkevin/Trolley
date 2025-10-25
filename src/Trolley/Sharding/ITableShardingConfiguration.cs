namespace Trolley;

public interface ITableShardingConfiguration
{
    void Configure(TableShardingBuilder builder);
}