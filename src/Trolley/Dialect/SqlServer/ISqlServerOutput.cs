using System;

namespace Trolley.SqlServer;

public interface ISqlServerOutput<TEntity>
{
    TField Inserted<TField>(Func<TEntity, TField> fieldsSelector);
    TFields Deleted<TFields>(Func<TEntity, TFields> fieldsSelector);
}