using System.Data;

namespace Trolley;

public delegate void CommandInitializer(IDbCommand command, IOrmProvider ormProvider, params object[] parameters);
public delegate void PagedCommandInitializer(IDbCommand command, int pageIndex, int pageSize, string orderBy, params object[] parameters);
