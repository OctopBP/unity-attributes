#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.GenConstructor;

internal sealed record ClassToProcess(ITypeSymbol ClassSymbol, List<IFieldSymbol> Fields)
{
    public string FullCsharpName { get; } = ClassSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(ClassSymbol);
        
    public ITypeSymbol ClassSymbol { get; } = ClassSymbol;
    public List<IFieldSymbol> Fields { get; } = Fields;
}