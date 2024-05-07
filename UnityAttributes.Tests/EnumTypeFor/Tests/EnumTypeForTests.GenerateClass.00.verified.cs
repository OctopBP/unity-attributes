﻿//HintName: EnumTypeForAttribute.g.cs
/// <auto-generated />

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("UnityAttributes", "1.4.0.0")]
[global::System.AttributeUsage(global::System.AttributeTargets.Enum, Inherited = true, AllowMultiple = true)]
internal sealed class EnumTypeForAttribute : global::System.Attribute
{
    private System.Type _type;
    private string _shortName;
    private bool _unitySerializable;

    internal EnumTypeForAttribute(System.Type type, string shortName = null, bool unitySerializable = true)
    {
        _type = type;
        _shortName = shortName;
        _unitySerializable = unitySerializable;
    }
}
