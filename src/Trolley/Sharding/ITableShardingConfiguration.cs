namespace Trolley;

public interface ITableShardingConfiguration
{
    void OnModelCreating(TableShardingBuilder builder);
}