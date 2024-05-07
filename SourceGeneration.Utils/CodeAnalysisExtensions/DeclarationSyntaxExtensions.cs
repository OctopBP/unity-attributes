using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGeneration.Utils.CodeAnalysisExtensions;

public static class DeclarationSyntaxExtensions
{
    public static bool HaveAttribute(this MemberDeclarationSyntax enumDeclarationSyntax, string attributeName)
    {
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.AttributeIsEqualByName(attributeName))
                {
                    return true;
                }
            }
        }
    
        return false;
    }
    
    public static List<AttributeSyntax> AllAttributesWithName(this MemberDeclarationSyntax enumDeclarationSyntax, string attributeName)
    {
        List<AttributeSyntax> list = [];
        
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.AttributeIsEqualByName(attributeName))
                {
                    list.Add(attributeSyntax);
                }
            }
        }
    
        return list;
    }
    
    public static List<AttributeSyntax> AllAttributesWhere(this MemberDeclarationSyntax enumDeclarationSyntax, Predicate<AttributeSyntax> filter)
    {
        List<AttributeSyntax> list = [];
        
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (filter(attributeSyntax))
                {
                    list.Add(attributeSyntax);
                }
            }
        }
    
        return list;
    }
    
    public static bool HaveAttributeWithArguments(this MemberDeclarationSyntax enumDeclarationSyntax, string attributeName, out AttributeArgumentListSyntax argumentList)
    {
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (!attributeSyntax.Name.AttributeIsEqualByName(attributeName))
                {
                    continue;
                }

                if (attributeSyntax.ArgumentList is not { Arguments.Count: > 0, })
                {
                    continue;
                }

                argumentList = attributeSyntax.ArgumentList;
                return true;
            }
        }

        argumentList = default;
        return false;
    }

    public static bool HaveInterface(this ITypeSymbol typeSymbol, string interfaceName)
    {
        foreach (var typeInterface in typeSymbol.Interfaces)
        {
            if (typeInterface.ToDisplayString() == interfaceName)
            {
                return true;
            }
        }

        return false;
    }
    
    public static bool HaveInterface(this ClassDeclarationSyntax classDeclarationSyntax, string interfaceName)
    {
        if (classDeclarationSyntax.BaseList is null)
        {
            return false;
        }
        
        foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList.Types)
        {
            if (baseTypeSyntax.Type.ToString() == interfaceName)
            {
                return true;
            }
        }

        return false;
    }
}