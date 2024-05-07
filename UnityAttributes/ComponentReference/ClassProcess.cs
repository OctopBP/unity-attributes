#nullable enable
using Microsoft.CodeAnalysis;

namespace UnityAttributes.ComponentReference;

internal sealed record ClassProcess(ITypeSymbol ClassSymbol)
{
    public string FullCsharpName { get; } = ClassSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(ClassSymbol);
        
    public ITypeSymbol ClassSymbol { get; } = ClassSymbol;
}