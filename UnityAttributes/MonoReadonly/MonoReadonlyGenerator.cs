using Microsoft.CodeAnalysis;

namespace UnityAttributes.MonoReadonly;

[Generator]
public partial class MonoReadonlyGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForPostInitialization(i => i.AddSource($"{AttributeName}.g.cs", attributeText));
	}

	public void Execute(GeneratorExecutionContext context) {}
}