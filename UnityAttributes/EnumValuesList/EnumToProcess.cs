#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.EnumValuesList;

internal sealed record EnumToProcess(ITypeSymbol EnumSymbol, List<EnumMemberToProcess> Members,
    string? FullNamespace)
{
    public string FullCsharpName { get; } = EnumSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(EnumSymbol);
        
    public ITypeSymbol EnumSymbol { get; } = EnumSymbol;
    public List<EnumMemberToProcess> Members { get; } = Members;
    public string? FullNamespace { get; } = FullNamespace;
}