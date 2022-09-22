using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley;

public class EntityMap
{
    private readonly Dictionary<string, MemberMap> memberMaps = new();

    public EntityMap(Type entityType) => this.EntityType = entityType;

    public string EntityName { get; set; }
    public Type EntityType { get; set; }
    public string TableName { get; set; }
    public string FieldPrefix { get; set; } = String.Empty;
    public bool IsNullable { get; set; }
    public Type UnderlyingType { get; set; }
    public bool IsValueTuple { get; set; }
    public bool IsAutoIncrement { get; set; }

    public List<string> KeyFields { get; set; }
    public ICollection<MemberMap> MemberMaps => this.memberMaps.Values;
    public string AutoIncrementField { get; set; }

    public void SetKeys(params MemberInfo[] memberInfos)
    {
        foreach (var memberInfo in memberInfos)
        {
            if (!this.memberMaps.TryGetValue(memberInfo.Name, out var memberMap))
            {
                memberMap = new MemberMap(this, this.FieldPrefix, memberInfo);
                this.memberMaps.TryAdd(memberInfo.Name, memberMap);
            }
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
        if (this.memberMaps.TryGetValue(memberName, out var mapper))
            return mapper;
        var memberInfos = this.EntityType.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (memberInfos == null || memberInfos.Length <= 0)
            throw new Exception($"不存在名为{memberName}的成员");
        return new MemberMap(this, this.FieldPrefix, memberInfos[0]);
    }
    public List<MemberInfo> GetMembers() => this.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
    public void AddMemberMap(string memberName, MemberMap mapper)
        => this.memberMaps.TryAdd(memberName, mapper);
    public EntityMap Build()
    {
        if (string.IsNullOrEmpty(this.EntityName))
            this.EntityName = this.EntityType.Name;
        if (string.IsNullOrEmpty(this.TableName))
            this.TableName = this.EntityType.Name;

        if (this.EntityType.IsValueType)
        {
            this.UnderlyingType = Nullable.GetUnderlyingType(this.EntityType);
            this.IsNullable = this.UnderlyingType != null;
            if (!this.IsNullable)
                this.UnderlyingType = this.EntityType;

            this.IsValueTuple = this.UnderlyingType.FullName.StartsWith("System.ValueTuple`");
        }

        if (this.memberMaps.Count > 0)
        {
            this.KeyFields = new List<string>();
            foreach (var memberMapper in this.memberMaps.Values)
            {
                var fieldName = $"{this.FieldPrefix}{memberMapper.FieldName}";
                if (memberMapper.IsKey)
                    this.KeyFields.Add(fieldName);
                if (memberMapper.IsAutoIncrement)
                    this.AutoIncrementField = fieldName;
            }
        }
        return this;
    }
    public static EntityMap CreateDefaultMap(Type entityType)
    {
        var mapper = new EntityMap(entityType);
        mapper.EntityName = entityType.Name;
        mapper.TableName = entityType.Name;

        if (entityType.IsValueType)
        {
            mapper.UnderlyingType = Nullable.GetUnderlyingType(entityType);
            mapper.IsNullable = mapper.UnderlyingType != null;
            if (!mapper.IsNullable)
                mapper.UnderlyingType = entityType;

            mapper.IsValueTuple = mapper.UnderlyingType.FullName.StartsWith("System.ValueTuple`");
        }
        MemberInfo[] memberInfos = null;
        if (mapper.IsValueTuple)
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
}