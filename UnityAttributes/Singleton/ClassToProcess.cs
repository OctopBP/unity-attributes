#nullable enable
using Microsoft.CodeAnalysis;

namespace UnityAttributes.Singleton;

internal sealed record ClassToProcess(ITypeSymbol ClassSymbol, string? FullNamespace, string MethodName)
{
    public string FullCsharpName { get; } = ClassSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(ClassSymbol);
        
    public ITypeSymbol ClassSymbol { get; } = ClassSymbol;
    public string? FullNamespace { get; } = FullNamespace;
    public string MethodName { get; } = MethodName;
}