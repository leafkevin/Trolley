using System;

namespace Trolley;

/// <summary>
/// 类型处理器提供者
/// </summary>
public interface ITypeHandlerProvider
{
    /// <summary>
    /// 添加类型处理器
    /// </summary>
    /// <param name="typeHandler">添加类型处理器</param>
    void AddTypeHandler(ITypeHandler typeHandler);
    /// <summary>
    /// 获取类型处理器
    /// </summary>
    /// <param name="handlerType">类型处理器的<c>Type</c>类型</param>
    /// <param name="typeHandler"></param>
    /// <returns></returns>
    bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler);
}
