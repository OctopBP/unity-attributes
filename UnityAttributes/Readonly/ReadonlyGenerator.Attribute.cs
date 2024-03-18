using UnityAttributes.Common;

namespace UnityAttributes.Readonly;

public partial class ReadonlyGenerator
{
	public const string AttributeName = "ReadonlyAttribute";
	static readonly string generatedCodeAttribute = typeof(ReadonlyGenerator).GeneratedCodeAttribute();
	
	readonly string attributeText = @$"{Const.AUTO_GENERATED_TEXT}

{generatedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal class {AttributeName} : global::System.Attribute
{{
  public {AttributeName}() {{ }}
}}
";
}