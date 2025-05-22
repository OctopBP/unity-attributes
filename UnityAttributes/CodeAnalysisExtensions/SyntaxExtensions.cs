#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGeneration.Utils.Common;

namespace SourceGeneration.Utils.CodeAnalysisExtensions;

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
    
    public static Optional<string> GetNameText(this Optional<NameSyntax> name)
    {
        return name.HasValue
            ? name.Value switch
            {
                SimpleNameSyntax ins => ins.Identifier.Text,
                QualifiedNameSyntax qns => qns.Right.Identifier.Text,
                _ => OptionalExt.None<string>(),
            }
            : OptionalExt.None<string>();
    }
}