using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.ShaderProperty;

public static class ShaderPropertyAttribute
{
    public const string AttributeName = "ShaderProperty";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.Attribute(AttributeName, typeof(ShaderPropertyGenerator), AttributeTargets.Class, true,
        [
            ("string", "name", null),
            ($"{ShaderPropertyType.EnumFullName}", "type", null),
            ($"{ShaderPropertyMode.EnumFullName}", "mode", $"{ShaderPropertyMode.EnumFullName}.Default")
        ]);
}


