using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.Alias;

public class AliasAttribute
{
    public const string AttributeName = "Alias";
    public const string ForTypeAttributeName = "forType";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText = Utils.Attribute(
        AttributeName, typeof(AliasGenerator), AttributeTargets.Class, false,
        [("System.Type", ForTypeAttributeName, null)]
    );
}