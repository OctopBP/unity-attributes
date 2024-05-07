using Microsoft.CodeAnalysis;

namespace UnityAttributes.MonoReadonly;

[Generator]
public partial class MonoReadonlyGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForPostInitialization(i => i.AddSource($"{AttributeFullName}.g.cs", AttributeText));
	}

	public void Execute(GeneratorExecutionContext context) {}
}