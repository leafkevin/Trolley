using System;

namespace Trolley.SqlServer;

public interface ISqlServerOutput<TEntity>
{
    TFields Inserted<TFields>(Func<TEntity, TFields> fieldsSelector);
    TFields Deleted<TFields>(Func<TEntity, TFields> fieldsSelector);
}