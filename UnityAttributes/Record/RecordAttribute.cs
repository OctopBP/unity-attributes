using System;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.Record;

public class RecordAttribute
{
    public const string AttributeName = "Record";
    public static readonly string AttributeFullName = AttributeName.WithAttributePostfix();
    public static readonly string AttributeText = Utils.SimpleAttribute(AttributeName, AttributeTargets.Class, false);
}