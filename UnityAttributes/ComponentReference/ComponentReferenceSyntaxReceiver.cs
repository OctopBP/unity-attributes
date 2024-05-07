using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityAttributes.ComponentReference;

internal class ComponentReferenceSyntaxReceiver : ISyntaxContextReceiver
{
    public List<(INamedTypeSymbol classSymbol, IFieldSymbol[] fieldsSymbols)> Classes { get; } = [];

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax) return;

        var classSymbol = Unsafe.As<INamedTypeSymbol>(
            context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)
        );
        
        var haveAttribute =
            classSymbol?.GetAttributes().Any(ad =>
                ad.AttributeClass?.ToDisplayString() == ComponentReferenceAttribute.AttributeName
            ) ?? false;

        if (!haveAttribute) return;
        
        var fields = classDeclarationSyntax
            .ChildNodes()
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Variables)
            .Select(variable => Unsafe.As<IFieldSymbol>(context.SemanticModel.GetDeclaredSymbol(variable)))
            .Where(variable => !variable.IsConst)
            .ToArray();
            
        Classes.Add((classSymbol, fields));
    }
}