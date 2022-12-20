using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley;

public class EntityMap
{
    private readonly ConcurrentDictionary<string, MemberMap> memberMaps = new();

    public EntityMap(Type entityType) => this.EntityType = entityType;

    public Type EntityType { get; set; }
    public string TableName { get; set; }
    public string FieldPrefix { get; set; } = String.Empty;
    public bool IsNullable { get; set; }
    public Type UnderlyingType { get; set; }
    public bool IsAutoIncrement { get; set; }

    public List<MemberMap> KeyMembers { get; set; }
    public ICollection<MemberMap> MemberMaps => this.memberMaps.Values;
    public string AutoIncrementField { get; set; }

    public void SetKeys(params MemberInfo[] memberInfos)
    {
        this.KeyMembers = new List<MemberMap>();
        foreach (var memberInfo in memberInfos)
        {
            if (!this.memberMaps.TryGetValue(memberInfo.Name, out var memberMap))
            {
                memberMap = new MemberMap(this, this.FieldPrefix, memberInfo);
                this.memberMaps.TryAdd(memberInfo.Name, memberMap);
            }
            this.KeyMembers.Add(memberMap);
            memberMap.IsKey = true;
        }
    }
    public void SetAutoIncrement(MemberInfo memberInfo)
    {
        if (!this.memberMaps.TryGetValue(memberInfo.Name, out var memberMap))
            this.memberMaps.TryAdd(memberInfo.Name, memberMap = new MemberMap(this, this.FieldPrefix, memberInfo));
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
        //this.AddMemberMap(memberName, mapper = new MemberMap(this, this.FieldPrefix, memberInfos[0]));
        //除了指定映射，其他成员映射不做缓存，根据成员信息现生成映射
        return new MemberMap(this, this.FieldPrefix, memberInfos[0]);
    }

    public void AddMemberMap(string memberName, MemberMap mapper)
        => this.memberMaps.TryAdd(memberName, mapper);
    public EntityMap Build()
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
        //不补充其他未配置的列映射，在生成SQL的时候，动态获取，减少映射内存占用
        //var memberInfos = this.GetMembers();
        //foreach (var memberInfo in memberInfos)
        //{
        //    if (!this.TryGetMemberMap(memberInfo.Name, out _))
        //        this.AddMemberMap(memberInfo.Name, new MemberMap(this, this.FieldPrefix, memberInfo));
        //}
        if (this.memberMaps.Count > 0)
        {
            this.KeyMembers = new List<MemberMap>();
            foreach (var memberMapper in this.memberMaps.Values)
            {
                var fieldName = $"{this.FieldPrefix}{memberMapper.FieldName}";
                if (memberMapper.IsKey)
                    this.KeyMembers.Add(memberMapper);
                if (memberMapper.IsAutoIncrement)
                    this.AutoIncrementField = fieldName;
            }
        }
        return this;
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
                mapper.memberMaps.TryAdd(memberMapper.MemberName, memberMapper);
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
                mapper.memberMaps.TryAdd(memberMapper.MemberName, memberMapper);
            }
        }
        return mapper;
    }
    public List<MemberInfo> GetMembers() => this.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
}