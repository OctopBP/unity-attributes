using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityAttributes.Common;

namespace UnityAttributes.GenConstructor; 

[Generator]
public partial class GenConstructorGenerator : ISourceGenerator
{
    private static readonly string generatedCodeAttribute = typeof(GenConstructorGenerator).GeneratedCodeAttribute();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(i => { 
            i.AddSource($"{AttributeName}.g.cs", attributeText);
            i.AddSource($"{IgnoreAttributeName}.g.cs", ignoreAttributeText);
        });
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var (classSymbol, fieldsSymbols) in receiver.Classes)
        {
            var classSource = ProcessClass(classSymbol, fieldsSymbols);
            context.AddSource($"{classSymbol}.constructor.g.cs", SourceText.From(classSource, Encoding.UTF8));
        }
    }

    private static string ProcessClass(INamedTypeSymbol classSymbol, IFieldSymbol[] fieldsSymbols)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent(Const.AUTO_GENERATED_TEXT);
        builder.AppendLine();
        builder.AppendLine();
        
        if (!classSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.AppendIdent().Append("namespace ")
                .AppendLine(string.Join(".", classSymbol.ContainingNamespace.ConstituentNamespaces));
            builder.OpenBrackets();
        }

        var containingTypes = new List<INamedTypeSymbol>();
        var containingType = classSymbol.ContainingType;
        var indentCount = 0;
        while (containingType != null)
        {
            containingTypes.Add(containingType);
            containingType = containingType.ContainingType;
            indentCount++;
        }

        containingTypes.Reverse();
        foreach (var type in containingTypes)
        {
            builder.AppendIdent().Append("public partial class ").AppendLine("type.Name");
            builder.OpenBrackets();
        }
        
        builder.AppendIdent().Append("public partial class ").AppendLine(classSymbol.Name);
        builder.OpenBrackets();

        var fieldsParam = string.Join(", ", fieldsSymbols.Select(f => $"{f.Type} {f.Name}"));

        builder.AppendIdent().Append("public ").Append(classSymbol.Name).Append("(").Append(fieldsParam).AppendLine(")");
        builder.OpenBrackets();
        foreach (var fieldSymbol in fieldsSymbols)
        {
            ProcessField(builder, fieldSymbol);
        }
        builder.CloseBrackets();
        
        for (var j = 0; j < indentCount; j++)
        {
            builder.CloseBrackets();
        }
        
        if (!classSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.CloseBrackets();
        }
        builder.CloseBrackets();
        
        return builder.ToString();
        
        static void ProcessField(CodeBuilder codeBuilder, IFieldSymbol fieldSymbol)
        {
            var fieldName = fieldSymbol.Name;
            codeBuilder.AppendIdent().Append("this.").Append(fieldName).Append(" = ").Append(fieldName).AppendLine(";");
        }
    }
}

internal class SyntaxReceiver : ISyntaxContextReceiver
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
                ad.AttributeClass?.ToDisplayString() == GenConstructorGenerator.AttributeName
            ) ?? false;

        if (!haveAttribute) return;
        
        var fields = classDeclarationSyntax
            .ChildNodes()
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Variables)
            .Select(variable => Unsafe.As<IFieldSymbol>(context.SemanticModel.GetDeclaredSymbol(variable)))
            .Where(CheckForIgnoreAttribute)
            .Where(variable => !variable.IsConst)
            .ToArray();
            
        Classes.Add((classSymbol, fields));

        return;

        bool CheckForIgnoreAttribute(IFieldSymbol fieldSymbol) {
            var haveIgnoreAttribute = fieldSymbol?.GetAttributes()
                .Any(ad => ad.AttributeClass?.ToDisplayString() == GenConstructorGenerator.IgnoreAttributeName) ?? false;
            return !haveIgnoreAttribute;
        }
    }
}