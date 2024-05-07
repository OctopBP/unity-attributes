using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGeneration.Utils.CodeAnalysisExtensions;
using SourceGeneration.Utils.CodeBuilder;
using SourceGeneration.Utils.Common;

namespace UnityAttributes.EnumTypeFor;

[Generator]
public sealed class EnumTypeForGenerator : IIncrementalGenerator
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
            $"{EnumTypeForAttribute.AttributeFullName}.g", EnumTypeForAttribute.AttributeText));
        
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

        var list = new List<EnumToProcess>();
        foreach (var attributeSyntax in enumDeclarationSyntax.AllAttributesWhere
                     (syntax => syntax.Name.AttributeIsEqualByName(EnumTypeForAttribute.AttributeName)))
        {
            if (attributeSyntax.ArgumentList is null)
            {
                continue;
            }

            var arguments = attributeSyntax.ArgumentList.Arguments;
            if (arguments.Count == 0)
            {
                continue;
            }
            
            if (arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpressionSyntax)
            {
                continue;
            }

            var forTypeSymbol = ctx.SemanticModel.GetSymbolInfo(typeOfExpressionSyntax.Type).Symbol;
            if (forTypeSymbol is null)
            {
                continue;
            }

            // TODO: Rework this logic
            string customName = null;
            var unitySerializable = true;
            if (arguments.Count > 1)
            {
                var argument = arguments[1];
                if (argument.NameColon is not null
                    && argument.NameColon.Name.GetNameText() == "unitySerializable"
                    && argument.Expression is LiteralExpressionSyntax { Token.Text: "false" })
                {
                    unitySerializable = false;
                }
                else if (argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                {
                    customName = literalExpressionSyntax.Token.Text;
                }
            }
            
            if (arguments.Count > 2)
            {
                var argument = arguments[2];
                if (argument.NameColon is not null
                    && argument.NameColon.Name.GetNameText() == "shortName"
                    && argument.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                {
                    customName = literalExpressionSyntax.Token.Text;
                }
                else if (argument.Expression is LiteralExpressionSyntax { Token.Text: "false" })
                {
                    unitySerializable = false;
                }
            }

            list.Add(new EnumToProcess(enumDeclarationTypeSymbol, forTypeSymbol, membersToProcess,
                enumNamespace, customName, unitySerializable));
        }

        return list;
    }

    private static void GenerateCode(SourceProductionContext context, EnumToProcess enumToProcess)
    {
        var code = GenerateCode(enumToProcess);
        context.AddSource($"{enumToProcess.ClassName}.g", SourceText.From(code, Encoding.UTF8));
    }

    private static string GenerateCode(EnumToProcess enumToProcess)
    {
        var builder = new CodeBuilder();
        
        var isVisible = enumToProcess.EnumSymbol.IsVisibleOutsideOfAssembly();
        var methodVisibility = isVisible ? "public" : "internal";

        var typeName = enumToProcess.ForTypeSymbol.ToDisplayString();
        
        builder.AppendLine("/// <auto-generated />").AppendLine();
        
        if (!string.IsNullOrEmpty(enumToProcess.FullNamespace))
        {
            builder.Append("namespace ").Append(enumToProcess.FullNamespace!).AppendLine();
            builder.OpenBrackets();
        }

        if (enumToProcess.UnitySerializable)
        {
            builder.AppendLineWithIdent("[System.Serializable]");
        }
        builder.AppendIdent().Append(methodVisibility).Append(" class ").AppendLine(enumToProcess.ClassName);
        builder.OpenBrackets();

        Fields();
        DefaultConstructor();
        Constructor();
        Get();
        Set();
        Apply();

        builder.CloseBrackets();

        if (!string.IsNullOrEmpty(enumToProcess.FullNamespace))
        {
            builder.CloseBrackets();
        }

        return builder.ToString();
        
        void Fields()
        {
            foreach (var member in enumToProcess.Members)
            {
                builder.AppendIdent();
                if (enumToProcess.UnitySerializable)
                {
                    builder.Append("[UnityEngine.SerializeField] ");
                }
                builder.Append("private ").Append(typeName).Append(" ").Append(member.Name).AppendLine(";");
            }
        }
        
        void DefaultConstructor()
        {
            builder.AppendLine();
            builder.AppendIdent().Append("public ").Append(enumToProcess.ClassName).Append("() { }").AppendLine();
        }
        
        void Constructor()
        {
            builder.AppendLine();
            builder
                .AppendIdent().Append("public ").Append(enumToProcess.ClassName)
                .Append("(")
                .Append(string.Join(", ", enumToProcess.Members.Select(member => $"{typeName} {member.Name.FirstCharToLower()}")))
                .Append(")")
                .AppendLine();
        
            builder.OpenBrackets();
            foreach (var member in enumToProcess.Members)
            {
                builder
                    .AppendIdent().Append("this.").Append(member.Name).Append(" = ")
                    .Append(member.Name.FirstCharToLower()).Append(";")
                    .AppendLine();
            }
            builder.CloseBrackets();
        }

        void Get()
        {
            builder.AppendLine();
            builder
                .AppendIdent().Append("public ").Append(typeName)
                .Append(" Get(").Append(enumToProcess.FullCsharpName).AppendLine(" key)");
            builder.OpenBrackets();
            builder.AppendLineWithIdent("return key switch");
            builder.OpenBrackets();
            foreach (var member in enumToProcess.Members)
            {
                builder.AppendIdent().Append(enumToProcess.FullCsharpName).Append(".").Append(member.Name)
                    .Append(" => ").Append(member.Name).AppendLine(",");
            }
            builder.AppendLineWithIdent("_ => throw new System.ArgumentOutOfRangeException(nameof(key), key, null),");
            builder.DecreaseIdent().AppendLineWithIdent("};");
            builder.CloseBrackets();
        }
        
        void Set()
        {
            builder.AppendLine();
            builder
                .AppendIdent().Append("public void Set(").Append(enumToProcess.FullCsharpName)
                .Append(" key, ").Append(typeName).AppendLine(" value)");
            builder.OpenBrackets();
            builder.AppendLineWithIdent("switch (key)");
            builder.OpenBrackets();
            foreach (var member in enumToProcess.Members)
            {
                builder.AppendIdent().Append("case ").Append(enumToProcess.FullCsharpName).Append(".").Append(member.Name)
                    .Append(": ").Append(member.Name).AppendLine(" = value; break;");
            }
            builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(key), key, null);");
            builder.DecreaseIdent().AppendLineWithIdent("}");
            builder.CloseBrackets();
        }
        
        void Apply()
        {
            builder.AppendLine();
            builder
                .AppendIdent().Append("public void Apply(").Append(enumToProcess.FullCsharpName)
                .Append(" key, System.Func<").Append(typeName).Append(", ").Append(typeName).AppendLine("> func)");
            builder.OpenBrackets();
            builder.AppendLineWithIdent("switch (key)");
            builder.OpenBrackets();
            foreach (var member in enumToProcess.Members)
            {
                builder.AppendIdent().Append("case ").Append(enumToProcess.FullCsharpName).Append(".").Append(member.Name)
                    .Append(": ").Append(member.Name).Append(" = func(").Append(member.Name).AppendLine("); break;");
            }
            builder.AppendLineWithIdent("default: throw new System.ArgumentOutOfRangeException(nameof(key), key, null);");
            builder.DecreaseIdent().AppendLineWithIdent("}");
            builder.CloseBrackets();
        }
    }
}