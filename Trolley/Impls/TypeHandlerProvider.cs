using System;
using System.Collections.Concurrent;

namespace Trolley;

public class TypeHandlerProvider : ITypeHandlerProvider
{
    private readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();
    public void AddTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.typeHandlers.TryAdd(typeHandler.GetType(), typeHandler);
    }
    public bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler)
    {
        if (handlerType == null)
            throw new ArgumentNullException(nameof(handlerType));

        return this.typeHandlers.TryGetValue(handlerType, out typeHandler);
    }
}
