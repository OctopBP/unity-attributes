#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.ShaderProperty;

internal sealed record ClassToProcess(ITypeSymbol ClassSymbol, List<PropertyToProcess> Properties)
{
    public string FullCsharpName { get; } = ClassSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(ClassSymbol);
        
    public ITypeSymbol ClassSymbol { get; } = ClassSymbol;
    public List<PropertyToProcess> Properties { get; } = Properties;
}


