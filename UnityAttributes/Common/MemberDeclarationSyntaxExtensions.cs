using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityAttributes.Common;

public static class MemberDeclarationSyntaxExtensions
{
    public static bool HaveAttribute(this MemberDeclarationSyntax enumDeclarationSyntax, string attributeName)
    {
        foreach (var attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (attributeSyntax.Name.IsEqualByName(attributeName))
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
                if (attributeSyntax.Name.IsEqualByName(attributeName))
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
                if (!attributeSyntax.Name.IsEqualByName(attributeName))
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
}