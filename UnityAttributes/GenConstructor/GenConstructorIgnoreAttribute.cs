using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.GenConstructor;

public class GenConstructorIgnoreAttribute
{
    public const string AttributeName = "GenConstructorIgnore";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText = Utils.SimpleAttribute(AttributeName, AttributeTargets.Field, false);
}