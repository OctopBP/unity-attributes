using UnityAttributes.Common;

namespace UnityAttributes.Singleton;

public partial class SingletonGenerator {
  public const string AttributeName = "SingletonAttribute";
  readonly string attributeText = @$"{Const.AUTO_GENERATED_TEXT}

{generatedCodeAttribute}
[global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class {AttributeName} : global::System.Attribute
{{
  public {AttributeName}() {{ }}
}}
";
}