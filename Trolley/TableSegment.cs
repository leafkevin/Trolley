﻿using System;
using System.Reflection;

namespace Trolley;

public class TableSegment
{
    public string JoinType { get; set; }
    public Type EntityType { get; set; }
    public string AliasName { get; set; }
    public TableSegment IncludedFrom { get; set; }
    public MemberInfo FromMember { get; set; }
    public string Body { get; set; }
    public EntityMap Mapper { get; set; }
    public bool IsInclude { get; set; } 
    public string Path { get; set; }
    public string Filter { get; set; }
    public string OnExpr { get; set; }
    public bool IsUsed { get; set; }
}