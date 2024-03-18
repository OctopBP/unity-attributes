using UnityAttributes.Common;

namespace UnityAttributes.GenConstructor;

public partial class GenConstructorGenerator {
  public const string IgnoreAttributeName = "GenConstructorIgnoreAttribute";
  readonly string ignoreAttributeText = @$"{Const.AUTO_GENERATED_TEXT}

{generatedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal sealed class {IgnoreAttributeName} : global::System.Attribute
{{
  public {IgnoreAttributeName}() {{ }}
}}
";
}