using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.Singleton;

public partial class SingletonGenerator
{
    public const string AttributeName = "Singleton";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText =
        Utils.SimpleAttribute(AttributeName, typeof(SingletonGenerator), AttributeTargets.Class, false);
}