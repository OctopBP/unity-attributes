using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.EnumTypeFor;

public static class EnumTypeForAttribute
{
    public const string AttributeName = "EnumTypeFor";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.Attribute(AttributeName, typeof(EnumTypeForGenerator), AttributeTargets.Enum, true, [
            ("System.Type", "type", null), ("string", "shortName", "null"),
            ("bool", "unitySerializable", "true")
        ]);
}