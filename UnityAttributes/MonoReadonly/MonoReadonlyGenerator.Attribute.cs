using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.MonoReadonly;

public partial class MonoReadonlyGenerator
{
	public const string AttributeName = "MonoReadonly";
	public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
	public static readonly string AttributeText =
		Utils.SimpleAttribute(AttributeName, typeof(MonoReadonlyGenerator), AttributeTargets.Field, false);
}