#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.PublicAccessor;

internal sealed record ClassToProcess(ITypeSymbol ClassSymbol, List<FieldToProcess> Fields)
{
    public string FullCsharpName { get; } = ClassSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(ClassSymbol);
        
    public ITypeSymbol ClassSymbol { get; } = ClassSymbol;
    public List<FieldToProcess> Fields { get; } = Fields;
}