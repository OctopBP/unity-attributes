using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.Singleton;

public class SingletonAttribute
{
    public const string AttributeName = "Singleton";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.Attribute(AttributeName, typeof(SingletonGenerator), AttributeTargets.Class, false,
            [("string", "InitMethodName", null)]);
}