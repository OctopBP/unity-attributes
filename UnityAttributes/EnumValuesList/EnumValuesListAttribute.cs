using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.EnumValuesList;

public static class EnumValuesListAttribute
{
    public const string AttributeName = "EnumValuesList";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText = Utils.SimpleAttribute(AttributeName, AttributeTargets.Enum);
}