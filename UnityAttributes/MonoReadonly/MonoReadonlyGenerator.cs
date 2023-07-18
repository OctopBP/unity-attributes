using Microsoft.CodeAnalysis;
using UnityAttributes.Common;

namespace UnityAttributes.MonoReadonly;

[Generator]
public partial class MonoReadonlyGenerator : ISourceGenerator
{
	static readonly string generatedCodeAttribute = typeof(MonoReadonlyGenerator).generatedCodeAttribute();

	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForPostInitialization(i => i.AddSource($"{AttributeName}.g.cs", attributeText));
	}

	public void Execute(GeneratorExecutionContext context) {}
}