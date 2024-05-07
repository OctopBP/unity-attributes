using Microsoft.CodeAnalysis;

namespace UnityAttributes.Readonly;

[Generator]
public partial class ReadonlyGenerator : ISourceGenerator
{
	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForPostInitialization(i => i.AddSource($"{AttributeName}.g.cs", AttributeText));
	}

	public void Execute(GeneratorExecutionContext context) {}
}