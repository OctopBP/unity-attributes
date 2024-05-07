using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.GenConstructor;

public class GenConstructorAttribute
{
    public const string AttributeName = "GenConstructor";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText = Utils.SimpleAttribute(AttributeName, AttributeTargets.Class, false);
}