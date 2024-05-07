using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.Readonly;

public partial class ReadonlyGenerator
{
	public const string AttributeName = "Readonly";
	public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
	public static readonly string AttributeText =
		Utils.SimpleAttribute(AttributeName, typeof(ReadonlyGenerator), AttributeTargets.Field, false);
}