using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.ComponentReference;

public abstract class ComponentReferenceAttribute
{ 
    public const string AttributeName = "ComponentReference";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.SimpleAttribute(AttributeName, typeof(ComponentReferenceGenerator), AttributeTargets.Class, false);
}