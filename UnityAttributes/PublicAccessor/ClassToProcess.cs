#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.PublicAccessor;

internal sealed record ClassToProcess(ITypeSymbol FieldSymbol, List<FieldToProcess> Fields, string? FullNamespace)
{
    public string FullCsharpName { get; } = FieldSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(FieldSymbol);
        
    public ITypeSymbol FieldSymbol { get; } = FieldSymbol;
    public List<FieldToProcess> Fields { get; } = Fields;
    public string? FullNamespace { get; } = FullNamespace;
}