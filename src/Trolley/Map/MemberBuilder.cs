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
        if (nativeDbType == null)
            throw new ArgumentNullException(nameof(nativeDbType));

        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> AutoIncrement(bool isAutoIncrement = true)
    {
        this.mapper.IsAutoIncrement = isAutoIncrement;
        return this;
    }
    public virtual MemberBuilder<TMember> Length(int length)
    {
        this.mapper.Length = length;
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
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.mapper.TypeHandler = typeHandler;
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
    public virtual MemberBuilder<TMember> Ignore(bool isIgnoreInsert, bool isIgnoreUpdate)
    {
        this.mapper.IsIgnoreInsert = isIgnoreInsert;
        this.mapper.IsIgnoreUpdate = isIgnoreUpdate;
        return this;
    }
}