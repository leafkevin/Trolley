using System;

namespace Trolley;

public class MemberBuilder<TMember>
{
    private readonly MemberMap mapper;

    public MemberBuilder(MemberMap mapper) => this.mapper = mapper;

    public virtual MemberBuilder<TMember> Name(string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
            throw new ArgumentNullException(nameof(memberName));

        this.mapper.MemberName = memberName;
        return this;
    }
    public virtual MemberBuilder<TMember> Field(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));

        this.mapper.FieldName = fieldName;
        return this;
    }
    public virtual MemberBuilder<TMember> DbColumnType(string dbColumnType)
    {
        if (string.IsNullOrEmpty(dbColumnType))
            throw new ArgumentNullException(nameof(dbColumnType));

        this.mapper.DbColumnType = dbColumnType;
        return this;
    }

    public virtual MemberBuilder<TMember> NativeDbType(object nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType ?? throw new ArgumentNullException(nameof(nativeDbType));
        return this;
    }
    public virtual MemberBuilder<TMember> AutoIncrement(bool isAutoIncrement = true)
    {
        this.mapper.IsAutoIncrement = isAutoIncrement;
        return this;
    }
    public virtual MemberBuilder<TMember> MaxLength(int length)
    {
        this.mapper.MaxLength = length;
        return this;
    }
    public virtual MemberBuilder<TMember> Required(bool isRequired = true)
    {
        this.mapper.IsRequired = isRequired;
        return this;
    }
    public virtual MemberBuilder<TMember> Position(int position)
    {
        this.mapper.Position = position;
        return this;
    }
    public virtual MemberBuilder<TMember> RowVersion(bool isRowVersion = true)
    {
        this.mapper.IsRowVersion = isRowVersion;
        return this;
    }
    public virtual MemberBuilder<TMember> TypeHandler(ITypeHandler typeHandler)
    {
        this.mapper.TypeHandler = typeHandler ?? throw new ArgumentNullException(nameof(typeHandler));
        return this;
    }
    public virtual MemberBuilder<TMember> TypeHandler<TTypeHandler>() where TTypeHandler : class, ITypeHandler, new()
    {
        this.mapper.TypeHandlerType = typeof(TTypeHandler);
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore(bool isIgnore = true)
    {
        this.mapper.IsIgnore = true;
        return this;
    }
    public virtual MemberBuilder<TMember> IgnoreInsert(bool isIgnoreInsert = true)
    {
        this.mapper.IsIgnoreInsert = isIgnoreInsert;
        return this;
    }
    public virtual MemberBuilder<TMember> IgnoreUpdate(bool isIgnoreUpdate = true)
    {
        this.mapper.IsIgnoreUpdate = isIgnoreUpdate;
        return this;
    }
}