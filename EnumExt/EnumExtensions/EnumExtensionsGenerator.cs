using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Utils.CodeAnalysisExtensions;
using SourceGeneration.Utils.CodeBuilder;

namespace EnumExt.EnumExtensions;

[Generator]
public sealed class EnumExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var enums = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (syntaxContext, token) => GetSemanticTargetForGeneration(syntaxContext, token))
            .SelectMany(static (array, _) => array)
            .Collect()
            .SelectMany(static (array, _) => array);

        context.RegisterPostInitializationOutput(i => i.AddSource(
            $"{EnumExtensionsAttribute.AttributeFullName}.g", EnumExtensionsAttribute.AttributeText));
        
        context.RegisterSourceOutput(enums, GenerateCode);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is EnumDeclarationSyntax;
    }
    
    private static List<EnumToProcess> GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx,
        CancellationToken token)
    {
        var enumDeclarationSyntax = (EnumDeclarationSyntax) ctx.Node;

        var enumDeclarationSymbol = ctx.SemanticModel.GetDeclaredSymbol(enumDeclarationSyntax, token);
        if (enumDeclarationSymbol is not ITypeSymbol enumDeclarationTypeSymbol)
        {
            return [];
        }

        var enumNamespace = enumDeclarationTypeSymbol.GetNamespace();
        
        var membersToProcess = new List<EnumMemberToProcess>();
        foreach (var enumMemberDeclarationSyntax in enumDeclarationSyntax.Members)
        { 
            membersToProcess.Add(new EnumMemberToProcess(enumMemberDeclarationSyntax.Identifier.Text));
        }

        if (!enumDeclarationSyntax.HaveAttribute(EnumExtensionsAttribute.AttributeName))
        {
            return [];
        }
        
        var list = new List<EnumToProcess>
        {
            new EnumToProcess(enumDeclarationTypeSymbol, membersToProcess, enumNamespace)
        };

        return list;
    }

    private static void GenerateCode(SourceProductionContext context, EnumToProcess enumToProcess)
    {
        var code = GenerateCode(enumToProcess);
        context.AddSource($"{enumToProcess.EnumSymbol.ToDisplayString()}_Extensions.g",
            SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(EnumToProcess enumToProcess)
    {
        var builder = new CodeBuilder();
        
        var isVisible = enumToProcess.EnumSymbol.IsVisibleOutsideOfAssembly();
        var methodVisibility = isVisible ? "public" : "internal";
        var enumName = enumToProcess.EnumSymbol.Name;
        var enumFullName = enumToProcess.FullCsharpName;
        
        builder.AppendLineWithIdent("/// <auto-generated />");
        builder.AppendLine();
        builder.AppendLineWithIdent("using LanguageExt;");
        
        using (new NamespaceBlock(builder, enumToProcess.EnumSymbol))
        {
            builder.AppendIdent().Append(methodVisibility).Append(" static partial class ")
                .Append(enumName).AppendLine("Ext");
            
            using (new BracketsBlock(builder))
            {
                AppendValues();
                builder.AppendLine();
                AppendName();
                builder.AppendLine();
                AppendFromString();
                builder.AppendLine();
                AppendTryFromString();
                builder.AppendLine();
                AppendFold();
                builder.AppendLine();
                AppendFoldT();
                builder.AppendLine();
                AppendValueFold();
            }
        }

        return builder.ToString();

        void AppendValues()
        {
            builder.AppendIdent().Append("public static ").Append(enumFullName).Append("[] Values => new[]").AppendLine();
            using (new BracketsBlock(builder, withSemicolon: true))
            {
                foreach (var member in enumToProcess.Members)
                {
                    builder.AppendIdent().Append(enumFullName).Append(".").Append(member.Name).Append(",").AppendLine();
                }
            }
        }
        
        void AppendName()
        {
            builder.AppendIdent().Append("public static string Name(this ")
                .Append(enumFullName).Append(" self)").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (self)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent().Append("case ").Append(enumFullName).Append(".")
                            .Append(member.Name).Append(": return \"").Append(member.Name).Append("\";").AppendLine();
                    }

                    builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(self), self, null);");
                }
            }
        }
        
        void AppendFromString()
        {
            builder.AppendIdent().Append("public static ").Append(enumFullName)
                .Append(" FromString(string value)").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (value)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent()
                            .Append("case \"").Append(member.Name).Append("\": return ")
                            .Append(enumFullName).Append(".").Append(member.Name).Append(";")
                            .AppendLine();
                    }

                    builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(value), value, null);");
                }
            }
        }
        
        void AppendTryFromString()
        {
            builder.AppendIdent().Append("public static Option<").Append(enumFullName)
                .Append("> TryFromString(string value)").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (value)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent()
                            .Append("case \"").Append(member.Name)
                            .Append("\": return Option<").Append(enumFullName).Append(">.Some(")
                            .Append(enumFullName).Append(".").Append(member.Name).Append(");")
                            .AppendLine();
                    }

                    builder.AppendIdent()
                        .Append("default: return Option<").Append(enumFullName).Append(">.None;")
                        .AppendLine();
                }
            }
        }
        
        void AppendFold()
        {
            builder.AppendIdent().Append("public static void Fold(this ")
                .Append(enumFullName).Append(" self");
            foreach (var member in enumToProcess.Members)
            {
                builder.Append(", System.Action on").Append(member.Name).Append(" = null");
            }
            builder.Append(")").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (self)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent().Append("case ").Append(enumFullName).Append(".")
                            .Append(member.Name).Append(": on").Append(member.Name).Append("?.Invoke(); return;")
                            .AppendLine();
                    }

                    builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(self), self, null);");
                }
            }
        }
        
        void AppendFoldT()
        {
            builder.AppendIdent().Append("public static T Fold<T>(this ")
                .Append(enumFullName).Append(" self");
            foreach (var member in enumToProcess.Members)
            {
                builder.Append(", System.Func<T> on").Append(member.Name);
            }
            builder.Append(")").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (self)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent().Append("case ").Append(enumFullName).Append(".")
                            .Append(member.Name).Append(": return on").Append(member.Name).Append(".Invoke();")
                            .AppendLine();
                    }
                    
                    builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(self), self, null);");
                }
            }
        }
        
        void AppendValueFold()
        {
            builder.AppendIdent().Append("public static T Fold<T>(this ")
                .Append(enumFullName).Append(" self");
            foreach (var member in enumToProcess.Members)
            {
                builder.Append(", T on").Append(member.Name);
            }
            builder.Append(")").AppendLine();
            
            using (new BracketsBlock(builder))
            {
                builder.AppendLineWithIdent("switch (self)");
                using (new BracketsBlock(builder))
                {
                    foreach (var member in enumToProcess.Members)
                    {
                        builder.AppendIdent().Append("case ").Append(enumFullName).Append(".")
                            .Append(member.Name).Append(": return on").Append(member.Name).Append(";")
                            .AppendLine();
                    }
                    
                    builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(self), self, null);");
                }
            }
        }
    }
}