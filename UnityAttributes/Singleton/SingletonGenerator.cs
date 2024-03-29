using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityAttributes.Common;

namespace UnityAttributes.Singleton; 

[Generator]
public partial class SingletonGenerator : ISourceGenerator
{
    private static readonly string generatedCodeAttribute = typeof(SingletonGenerator).GeneratedCodeAttribute();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(i => i.AddSource($"{AttributeName}.g.cs", attributeText));
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
            return;

        foreach (var classSymbol in receiver.Classes)
        {
            var classSource = ProcessClass(classSymbol);
            context.AddSource($"{classSymbol}.singleton.g.cs", SourceText.From(classSource, Encoding.UTF8));
        }
    }

    private static string ProcessClass(INamedTypeSymbol classSymbol)
    {
        var builder = new CodeBuilder();

        builder.AppendLineWithIdent(Const.AUTO_GENERATED_TEXT);
        builder.AppendLine();
        builder.AppendLine();
        
        if (!classSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            builder.AppendLineWithIdent($"namespace {string.Join(".", classSymbol.ContainingNamespace.ConstituentNamespaces)}");
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
            builder.AppendLineWithIdent($"public partial class {type.Name}");
            builder.OpenBrackets();
        }
        
        builder.AppendLineWithIdent($"public partial class {classSymbol.Name}");
        builder.OpenBrackets();
        
        builder.AppendLineWithIdent($"public static {classSymbol.Name} Instance {{ get; private set; }}");
        builder.AppendLine();
        builder.AppendLineWithIdent("void InitSingleton()");
        builder.OpenBrackets();
        builder.AppendLineWithIdent("if (Instance != null && Instance != this)");
        builder.OpenBrackets();
        builder.AppendLineWithIdent("Destroy(this);");
        builder.CloseBrackets();
        builder.AppendLineWithIdent("else");
        builder.OpenBrackets();
        builder.AppendLineWithIdent("Instance = this;");
        builder.CloseBrackets(); 
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
    }
}

internal class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<INamedTypeSymbol> Classes { get; } = [];

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax) return;

        var classSymbol = Unsafe.As<INamedTypeSymbol>(
            context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)
        );
        
        var haveAttribute =
            classSymbol?.GetAttributes().Any(ad =>
                ad.AttributeClass?.ToDisplayString() == SingletonGenerator.AttributeName
            ) ?? false;
        
        if (haveAttribute)
        {
            Classes.Add(classSymbol);
        }
    }
}