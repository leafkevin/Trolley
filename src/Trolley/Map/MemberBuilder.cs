namespace Trolley;

public class MemberBuilder<TMember>
{
    private readonly MemberMap mapper;

    public MemberBuilder(MemberMap mapper) => this.mapper = mapper;

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
        this.mapper.nativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> SetTypeHandler<TTypeHandler>() where TTypeHandler : class, ITypeHandler, new()
    {
        this.mapper.typeHandlerType = typeof(TTypeHandler);
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
}