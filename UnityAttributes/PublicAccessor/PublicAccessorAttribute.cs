using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.PublicAccessor;

public static class PublicAccessorAttribute
{
    public const string AttributeName = "PublicAccessor";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.SimpleAttribute(AttributeName, typeof(PublicAccessorGenerator), AttributeTargets.Field, false);
}