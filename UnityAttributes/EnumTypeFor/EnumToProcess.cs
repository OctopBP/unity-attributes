#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.EnumTypeFor;

internal sealed record EnumToProcess(ITypeSymbol EnumSymbol, ISymbol ForTypeSymbol, List<EnumMemberToProcess> Members,
    string? FullNamespace, bool ShortName)
{
    public string FullCsharpName { get; } = EnumSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(EnumSymbol);
    public bool ShortName { get; } = ShortName;
        
    public ITypeSymbol EnumSymbol { get; } = EnumSymbol;
    public ISymbol ForTypeSymbol { get; } = ForTypeSymbol;
    public List<EnumMemberToProcess> Members { get; } = Members;
    public string? FullNamespace { get; } = FullNamespace;
    public string ClassName { get; } =
        $"{EnumSymbol.Name}For{(ShortName ? ForTypeSymbol.Name : ForTypeSymbol.ToDisplayString().Replace(".", "_"))}";
}