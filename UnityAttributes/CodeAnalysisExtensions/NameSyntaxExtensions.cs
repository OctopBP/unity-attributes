using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGeneration.Utils.Common;

namespace SourceGeneration.Utils.CodeAnalysisExtensions;

public static class NameSyntaxExtensions
{
    public static bool AttributeIsEqualByName(this NameSyntax syntax, string attributeName)
    {
        var nameText = syntax.GetNameText();
        return nameText == attributeName || nameText == attributeName.WithAttributePostfix();
    }
}