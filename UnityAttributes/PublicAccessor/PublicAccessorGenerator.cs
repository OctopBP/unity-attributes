using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Utils.CodeAnalysisExtensions;
using SourceGeneration.Utils.CodeBuilder;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.PublicAccessor;

[Generator]
public sealed class PublicAccessorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var enums = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .Collect()
            .SelectMany(static (array, _) => array.Collect());

        context.RegisterPostInitializationOutput(i => i.AddSource(
            $"{PublicAccessorAttribute.AttributeFullName}.g", PublicAccessorAttribute.AttributeText));
        
        context.RegisterSourceOutput(enums, GenerateCode!);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }
        
    private static Optional<ClassToProcess> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax) ctx.Node;
        var classDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, token);
        if (classDeclarationSymbol is not ITypeSymbol classTypeSymbol)
        {
            return OptionalExt.None<ClassToProcess>();
        }
        
        var fieldToProcess = new List<FieldToProcess>();
        foreach (var classMemberDeclarationSyntax in classDeclarationSyntax.Members)
        {
            if (!classMemberDeclarationSyntax.HaveAttribute(PublicAccessorAttribute.AttributeName))
            {
                continue;
            }

            if (classMemberDeclarationSyntax is not FieldDeclarationSyntax fieldDeclarationSyntax)
            {
                continue;
            }
            
            foreach (var variable in fieldDeclarationSyntax.Declaration.Variables)
            {
                if (ctx.SemanticModel.GetDeclaredSymbol(variable) is IFieldSymbol fieldSymbol)
                {
                    fieldToProcess.Add(new FieldToProcess(fieldSymbol));
                }
            }
        }

        if (fieldToProcess.Count == 0)
        {
            return OptionalExt.None<ClassToProcess>();
        }
            
        return new ClassToProcess(classTypeSymbol, fieldToProcess);
    }

    private static void GenerateCode(SourceProductionContext context, ClassToProcess classToProcess)
    {
        var code = GenerateCode(classToProcess);
        context.AddSource($"{classToProcess.FullCsharpName}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(ClassToProcess classToProcess)
    {
        var builder = new CodeBuilder();
        
        var isVisible = classToProcess.ClassSymbol.IsVisibleOutsideOfAssembly();
        var methodVisibility = isVisible ? "public" : "internal";
        
        builder.AppendLine("/// <auto-generated />").AppendLine();

        using (new NamespaceBlock(builder, classToProcess.ClassSymbol))
        {
            using (new ParentsBlock(builder, classToProcess.ClassSymbol))
            {
                builder.AppendIdent().Append(methodVisibility).Append(" partial class ")
                    .AppendLine(classToProcess.ClassSymbol.Name);
                using (new BracketsBlock(builder))
                {
                    builder.AppendArray(classToProcess.Fields.ToArray(), GenerateFieldAccessor, b => b.AppendLine());
                }
            }
        }

        return builder.ToString();
        
        void GenerateFieldAccessor(FieldToProcess field, CodeBuilder b)
        {
            var fieldName = field.FieldSymbol.Name;
            var fieldType = field.FieldSymbol.Type;

            b.AppendLineWithIdent($"/// Public accessor for <see cref=\"{fieldName}\"/>");
            b.AppendLineWithIdent($"/// <inheritdoc cref=\"{fieldName}\"/>");
            b.AppendIdent().Append("public ").Append(fieldType.ToDisplayString()).Append(" ")
                .Append(fieldName.UpperFirstCharOrAddUnderline()).Append(" => ").Append(fieldName).AppendLine(";");
        }
    }
}