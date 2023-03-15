using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley;

public class EntityMap
{
    private readonly ConcurrentDictionary<string, MemberMap> memberMaps = new();
    private List<MemberMap> memberMappers = new();

    public EntityMap(Type entityType) => this.EntityType = entityType;

    public Type EntityType { get; set; }
    public string TableName { get; set; }
    public string FieldPrefix { get; set; } = String.Empty;
    public bool IsNullable { get; set; }
    public Type UnderlyingType { get; set; }
    public bool IsAutoIncrement { get; set; }

    public List<MemberMap> KeyMembers { get; set; }
    public List<MemberMap> MemberMaps => this.memberMappers;
    public string AutoIncrementField { get; set; }

    public void SetKeys(params MemberInfo[] memberInfos)
    {
        this.KeyMembers = new List<MemberMap>();
        foreach (var memberInfo in memberInfos)
        {
            if (!this.memberMaps.TryGetValue(memberInfo.Name, out var memberMap))
            {
                memberMap = new MemberMap(this, this.FieldPrefix, memberInfo);
                this.AddMemberMap(memberInfo.Name, memberMap);
            }
            this.KeyMembers.Add(memberMap);
            memberMap.IsKey = true;
        }
    }
    public void SetAutoIncrement(MemberInfo memberInfo)
    {
        if (!this.memberMaps.TryGetValue(memberInfo.Name, out var memberMap))
        {
            memberMap = new MemberMap(this, this.FieldPrefix, memberInfo);
            this.AddMemberMap(memberInfo.Name, memberMap);
        }
        memberMap.IsAutoIncrement = true;
        this.IsAutoIncrement = true;
    }
    public bool TryGetMemberMap(string memberName, out MemberMap mapper)
    {
        if (this.memberMaps.TryGetValue(memberName, out mapper))
            return true;
        mapper = null;
        return false;
    }
    public MemberMap GetMemberMap(string memberName)
    {
        //导航属性，一定存在映射，有就直接返回了
        if (this.memberMaps.TryGetValue(memberName, out var mapper))
            return mapper;
        var memberInfos = this.EntityType.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (memberInfos == null || memberInfos.Length <= 0)
            throw new Exception($"不存在名为{memberName}的成员");
        this.AddMemberMap(memberName, mapper = new MemberMap(this, this.FieldPrefix, memberInfos[0]));
        return mapper;
    }

    public void AddMemberMap(string memberName, MemberMap mapper)
    {
        if (this.memberMaps.TryAdd(memberName, mapper))
            this.memberMappers.Add(mapper);
    }
    public void Build(IOrmProvider ormProvider, ITypeHandlerProvider typeHandlerProvider)
    {
        if (string.IsNullOrEmpty(this.TableName))
            this.TableName = this.EntityType.Name;

        if (this.EntityType.IsValueType)
        {
            this.UnderlyingType = Nullable.GetUnderlyingType(this.EntityType);
            this.IsNullable = this.UnderlyingType != null;
            if (!this.IsNullable)
                this.UnderlyingType = this.EntityType;
        }
        var memberInfos = this.GetMembers();
        foreach (var memberInfo in memberInfos)
        {
            if (!this.TryGetMemberMap(memberInfo.Name, out var memberMapper))
            {
                memberMapper = new MemberMap(this, this.FieldPrefix, memberInfo);
                this.AddMemberMap(memberInfo.Name, memberMapper);

                //检查导航属性和TypeHandler配置，在Build的时候，就把错误暴漏出来
                if (memberMapper.MemberType.IsEntityType() && (!memberMapper.IsIgnore && !memberMapper.IsNavigation && memberMapper.TypeHandler == null))
                    throw new Exception($"类{this.EntityType.FullName}的成员{memberInfo.Name}不是值类型，未配置为导航属性也没有配置TypeHandler，也不是忽略成员");
            }
            if (memberMapper.typeHandlerType != null)
            {
                if (!typeHandlerProvider.TryGetTypeHandler(memberMapper.typeHandlerType, out var typeHandler))
                    throw new Exception($"{memberMapper.typeHandlerType.FullName}类型TypeHandler没有注册");
                memberMapper.TypeHandler = typeHandler;
            }

            if (memberMapper.nativeDbType.HasValue)
                memberMapper.NativeDbType = ormProvider.GetNativeDbType(memberMapper.nativeDbType.Value);
        }
        if (this.memberMaps.Count > 0)
        {
            this.KeyMembers ??= new List<MemberMap>();
            foreach (var memberMapper in this.memberMappers)
            {
                if (!memberMapper.IsKey) continue;

                var fieldName = $"{this.FieldPrefix}{memberMapper.FieldName}";
                if (!this.KeyMembers.Contains(memberMapper))
                    this.KeyMembers.Add(memberMapper);
                if (memberMapper.IsAutoIncrement)
                {
                    this.AutoIncrementField = fieldName;
                    this.IsAutoIncrement = true;
                }
            }
        }
    }
    public static EntityMap CreateDefaultMap(Type entityType)
    {
        var mapper = new EntityMap(entityType);
        mapper.TableName = entityType.Name;

        bool isValueTuple = false;
        if (entityType.IsValueType)
        {
            mapper.UnderlyingType = Nullable.GetUnderlyingType(entityType);
            mapper.IsNullable = mapper.UnderlyingType != null;
            if (!mapper.IsNullable)
                mapper.UnderlyingType = entityType;

            isValueTuple = mapper.UnderlyingType.FullName.StartsWith("System.ValueTuple`");
        }
        MemberInfo[] memberInfos = null;
        if (isValueTuple)
            memberInfos = mapper.UnderlyingType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        else
        {
            memberInfos = mapper.EntityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0).ToArray();
        }

        if (memberInfos != null && memberInfos.Length > 0)
        {
            foreach (var memberInfo in memberInfos)
            {
                var memberMapper = new MemberMap(mapper, mapper.FieldPrefix, memberInfo);
                mapper.AddMemberMap(memberMapper.MemberName, memberMapper);
            }
        }
        return mapper;
    }
    public static EntityMap CreateDefaultMap(Type entityType, EntityMap mapTo)
    {
        if (entityType == mapTo.EntityType)
            return mapTo;

        var mapper = new EntityMap(entityType);
        mapper.FieldPrefix = mapTo.FieldPrefix;
        mapper.TableName = mapTo.TableName;
        mapper.IsAutoIncrement = mapTo.IsAutoIncrement;
        mapper.KeyMembers = mapTo.KeyMembers;
        mapper.AutoIncrementField = mapTo.AutoIncrementField;

        bool isValueTuple = false;
        if (entityType.IsValueType)
        {
            mapper.UnderlyingType = Nullable.GetUnderlyingType(entityType);
            mapper.IsNullable = mapper.UnderlyingType != null;
            if (!mapper.IsNullable)
                mapper.UnderlyingType = entityType;

            isValueTuple = mapper.UnderlyingType.FullName.StartsWith("System.ValueTuple`");
        }
        MemberInfo[] memberInfos = null;
        if (isValueTuple)
            memberInfos = mapper.UnderlyingType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        else
        {
            memberInfos = mapper.EntityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0).ToArray();
        }

        if (memberInfos != null && memberInfos.Length > 0)
        {
            foreach (var memberInfo in memberInfos)
            {
                var mapToMemberMapper = mapTo.GetMemberMap(memberInfo.Name);
                var memberMapper = mapToMemberMapper.Clone(mapper, mapper.FieldPrefix, memberInfo);
                mapper.AddMemberMap(memberMapper.MemberName, memberMapper);
            }
        }
        return mapper;
    }
    public List<MemberInfo> GetMembers() => this.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
}