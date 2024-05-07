#nullable enable
using Microsoft.CodeAnalysis;

namespace SourceGeneration.Utils.CodeAnalysisExtensions;

public static class SymbolExtensions
{
    public static string? GetNamespace(this ISymbol symbol)
    {
        string? result = null;
        var ns = symbol.ContainingNamespace;
        while (ns is not null && !ns.IsGlobalNamespace)
        {
            if (result is not null)
            {
                result = ns.Name + "." + result;
            }
            else
            {
                result = ns.Name;
            }

            ns = ns.ContainingNamespace;
        }

        return result;
    }
    
    public static bool IsVisibleOutsideOfAssembly(this ISymbol? symbol)
    {
        if (symbol is null)
        {
            return false;
        }
        
        if (symbol.DeclaredAccessibility != Accessibility.Public &&
            symbol.DeclaredAccessibility != Accessibility.Protected &&
            symbol.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
        {
            return false;
        }

        if (symbol.ContainingType is null)
        {
            return true;
        }

        return IsVisibleOutsideOfAssembly(symbol.ContainingType);
    }
}