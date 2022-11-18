using System.Data;

namespace Trolley;

public class MemberBuilder
{
    private MemberMap mapper;
    public MemberBuilder(MemberMap mapper) => this.mapper = mapper;
    public virtual MemberBuilder Name(string memberName)
    {
        this.mapper.MemberName = memberName;
        return this;
    }
    public virtual MemberBuilder Field(string fieldName)
    {
        this.mapper.FieldName = fieldName;
        return this;
    }
    public virtual MemberBuilder DbType(DbType dbType)
    {
        this.mapper.DbType = dbType;
        return this;
    }
    public virtual MemberBuilder NativeDbType(int nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
    public MemberMap Build() => this.mapper;
}

public class MemberBuilder<TMember>
{
    private MemberMap mapper;
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
    public virtual MemberBuilder<TMember> DbType(DbType dbType)
    {
        this.mapper.DbType = dbType;
        return this;
    }
    public virtual MemberBuilder<TMember> NativeDbType(int nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
    public MemberMap Build() => this.mapper;
}
