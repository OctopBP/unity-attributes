#nullable enable
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace UnityAttributes.EnumTypeFor;

internal sealed record EnumToProcess(ITypeSymbol EnumSymbol, ISymbol ForTypeSymbol, List<EnumMemberToProcess> Members,
    string? FullNamespace, string? CustomName, bool UnitySerializable)
{
    public string FullCsharpName { get; } = EnumSymbol.ToDisplayString();
    public string DocumentationId { get; } = DocumentationCommentId.CreateDeclarationId(EnumSymbol);
    public string? CustomName { get; } = CustomName;
    public bool UnitySerializable { get; } = UnitySerializable;
        
    public ITypeSymbol EnumSymbol { get; } = EnumSymbol;
    public ISymbol ForTypeSymbol { get; } = ForTypeSymbol;
    public List<EnumMemberToProcess> Members { get; } = Members;
    public string? FullNamespace { get; } = FullNamespace;
    public string ClassName { get; } = CustomName ??
        $"{EnumSymbol.Name}For{ForTypeSymbol.Name.Replace(".", "_").Replace("<", "_").Replace(">", "_")}";
}