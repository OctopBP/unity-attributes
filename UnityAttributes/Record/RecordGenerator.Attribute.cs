using System;
using UnityAttributes.Common;

namespace UnityAttributes.Record;

public partial class RecordGenerator {
  public const string AttributeName = "RecordAttribute";
  readonly string attributeText = @$"{Const.AutoGeneratedText}

{generatedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal class {AttributeName} : global::System.Attribute {{
  public {AttributeName}() {{ }}
}}
";
}