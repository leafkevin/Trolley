using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley;

public class EntityMap
{
    private bool isBuild = false;
    private readonly ConcurrentDictionary<string, MemberMap> memberMaps = new();
    private readonly ConcurrentDictionary<string, MemberMap> fieldMaps = new();
    private readonly ConcurrentDictionary<string, Func<string, object, string>> shardingStrategies = new();
    private List<MemberMap> memberMappers = new();

    public EntityMap(Type entityType) => this.EntityType = entityType;

    public Type EntityType { get; set; }
    public string TableName { get; set; }
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
                memberMap = new MemberMap(this, memberInfo);
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
            memberMap = new MemberMap(this, memberInfo);
            this.AddMemberMap(memberInfo.Name, memberMap);
        }
        memberMap.IsAutoIncrement = true;
    }
    public bool TryGetMemberMap(string memberName, out MemberMap mapper)
    {
        if (this.memberMaps.TryGetValue(memberName, out mapper))
            return true;
        mapper = null;
        return false;
    }
    public bool TryGetMemberMapByFieldName(string fieldName, out MemberMap mapper)
    {
        if (this.fieldMaps.TryGetValue(fieldName, out mapper))
            return true;
        mapper = null;
        return false;
    }
    public MemberMap GetMemberMapByFieldName(string fieldName)
    {
        //导航属性，一定存在映射，有就直接返回了
        if (this.fieldMaps.TryGetValue(fieldName, out var mapper))
            return mapper;
        return mapper;
    }
    public MemberMap GetMemberMap(string memberName)
    {
        //导航属性，一定存在映射，有就直接返回了
        if (this.memberMaps.TryGetValue(memberName, out var mapper))
            return mapper;
        var memberInfos = this.EntityType.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (memberInfos == null || memberInfos.Length <= 0)
            throw new Exception($"不存在名为{memberName}的成员");
        this.AddMemberMap(memberName, mapper = new MemberMap(this, memberInfos[0]));
        return mapper;
    }

    public void AddMemberMap(string memberName, MemberMap mapper)
    {
        if (this.memberMaps.TryAdd(memberName, mapper))
            this.memberMappers.Add(mapper);
    }
    public void Build(IOrmProvider ormProvider)
    {
        if (this.isBuild) return;
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
                memberMapper = new MemberMap(this, memberInfo);
                this.AddMemberMap(memberInfo.Name, memberMapper);

                //检查导航属性和TypeHandler配置，在Build的时候，就把错误暴漏出来
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsIgnore && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{this.EntityType.FullName}的成员{memberInfo.Name}不是值类型，未配置为导航属性也没有配置TypeHandler，也不是忽略成员");
            }
            if (memberMapper.NativeDbType == null)
            {
                //没有配置，就生成默认的数据库映射类型
                if (!memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsIgnore && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    memberMapper.NativeDbType = ormProvider.GetNativeDbType(memberMapper.MemberType);
            }
            if (!memberMapper.IsNavigation && string.IsNullOrEmpty(memberMapper.DbColumnType))
                throw new ArgumentNullException("DbColumnType栏位不能为空，必须配置");
            if (memberMapper.NativeDbType is int nativeDbType)
                memberMapper.NativeDbType = Enum.ToObject(ormProvider.NativeDbTypeType, nativeDbType);
            if (memberMapper.TypeHandler == null && !memberMapper.IsIgnore && !memberMapper.IsNavigation)
            {
                if (memberMapper.TypeHandlerType != null)
                    memberMapper.TypeHandler = ormProvider.CreateTypeHandler(memberMapper.TypeHandlerType);
                else
                {
                    var dbFieldType = ormProvider.MapDefaultType(memberMapper.NativeDbType);
                    memberMapper.TypeHandler = ormProvider.GetTypeHandler(memberMapper.MemberType, dbFieldType, memberMapper.IsRequired);
                }
            }
            this.fieldMaps.TryAdd(memberMapper.FieldName, memberMapper);
        }
        if (this.memberMaps.Count > 0)
        {
            this.KeyMembers ??= new List<MemberMap>();
            //生成在配置代码的时候就尽力排好序，不排序也可以，此处排序反而还错了      
            foreach (var memberMapper in this.memberMappers)
            {
                if (!memberMapper.IsKey) continue;

                var fieldName = memberMapper.FieldName;
                if (!this.KeyMembers.Contains(memberMapper))
                    this.KeyMembers.Add(memberMapper);
                if (memberMapper.IsAutoIncrement)
                    this.AutoIncrementField = fieldName;
            }
            if (this.KeyMembers.Count == 1 && this.KeyMembers[0].IsAutoIncrement)
                this.IsAutoIncrement = true;
        }
        this.isBuild = true;
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
            memberInfos = mapper.UnderlyingType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        else
        {
            memberInfos = mapper.EntityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0).ToArray();
        }

        if (memberInfos != null && memberInfos.Length > 0)
        {
            foreach (var memberInfo in memberInfos)
            {
                var memberMapper = new MemberMap(mapper, memberInfo);
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
            memberInfos = mapper.UnderlyingType.GetFields(BindingFlags.Public | BindingFlags.Instance);
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
                var memberMapper = mapToMemberMapper.Clone(mapper, memberInfo);
                mapper.AddMemberMap(memberMapper.MemberName, memberMapper);
            }
        }
        return mapper;
    }
    private List<MemberInfo> GetMembers() => this.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
}