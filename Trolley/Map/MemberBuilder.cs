using System;

namespace Trolley;

public class MemberBuilder<TMember>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly MemberMap mapper;
    public MemberBuilder(IOrmDbFactory dbFactory, MemberMap mapper)
    {
        this.dbFactory = dbFactory;
        this.mapper = mapper;
    }
    public virtual MemberBuilder<TMember> Name(string memberName)
    {
        this.mapper.MemberName = memberName;
        return this;
    }
    public virtual MemberBuilder<TMember> Field(string fieldName)
    {
        this.mapper.FieldName = fieldName;
        return this;
    }
    public virtual MemberBuilder<TMember> NativeDbType(int nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> SetTypeHandler<TTypeHandler>() where TTypeHandler : class, ITypeHandler, new()
    {
        if (!this.dbFactory.TryGetTypeHandler(typeof(TTypeHandler), out var typeHandler))
            throw new Exception($"{typeof(TTypeHandler).FullName}类型TypeHandler没有注册");
        this.mapper.TypeHandler = typeHandler;
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
    public MemberMap Build() => this.mapper;
}