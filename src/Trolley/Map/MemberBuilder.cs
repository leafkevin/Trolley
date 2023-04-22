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
    public virtual MemberBuilder<TMember> NativeDbType(object nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> AutoIncrement()
    {
        this.mapper.IsAutoIncrement = true;
        return this;
    }
    public virtual MemberBuilder<TMember> TypeHandler(ITypeHandler typeHandler)
    {
        this.mapper.TypeHandler = typeHandler;
        return this;
    }
    public virtual MemberBuilder<TMember> TypeHandler<TTypeHandler>() where TTypeHandler : class, ITypeHandler, new()
    {
        this.mapper.TypeHandler = new TTypeHandler();
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
}