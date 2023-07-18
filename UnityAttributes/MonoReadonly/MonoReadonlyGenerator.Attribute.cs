using UnityAttributes.Common;

namespace UnityAttributes.MonoReadonly;

public partial class MonoReadonlyGenerator {
	public const string AttributeName = "MonoReadonlyAttribute";
	readonly string attributeText = @$"{Const.AutoGeneratedText}

{generatedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal class {AttributeName} : global::System.Attribute {{
  public {AttributeName}() {{ }}
}}
";
}