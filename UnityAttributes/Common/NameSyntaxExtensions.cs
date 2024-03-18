using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityAttributes.Common;

public static class NameSyntaxExtensions
{
    public static bool IsEqualByName(this NameSyntax syntax, string attributeName)
    {
        var nameText = syntax.GetNameText();
        return nameText == attributeName || nameText == attributeName + "Attribute";
    }
}