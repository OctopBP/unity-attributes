#nullable enable
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityAttributes.Common;

public static class SyntaxExtensions
{
    public static string? GetNameText(this NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null,
        };
    }
}