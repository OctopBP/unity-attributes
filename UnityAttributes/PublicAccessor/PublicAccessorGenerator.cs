using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnityAttributes.Common;

namespace UnityAttributes.PublicAccessor;

[Generator]
public partial class PublicAccessorGenerator : ISourceGenerator {
  static readonly string generatedCodeAttribute = typeof(PublicAccessorGenerator).generatedCodeAttribute();

  public void Initialize(GeneratorInitializationContext context) {
    context.RegisterForPostInitialization(i => i.AddSource($"{AttributeName}.g.cs", attributeText));
    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
  }

  public void Execute(GeneratorExecutionContext context) {
    if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
      return;

    var groups = receiver.fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(
      f => f.ContainingType, SymbolEqualityComparer.Default
    );

    foreach (var group in groups) {
      var classSource = processClass(group.Key, group.ToArray());
      context.AddSource($"{group.Key.Name}.transformed.g.cs", SourceText.From(classSource, Encoding.UTF8));
    }
  }

  static string processClass(INamedTypeSymbol classSymbol, IFieldSymbol[] fields) {
    var codeBuilder = new CodeBuilder();

    codeBuilder.append(Const.AutoGeneratedText);
    codeBuilder.appendEmptyLine();

    if (!classSymbol.ContainingNamespace.IsGlobalNamespace) {
      codeBuilder.appendLine(
        text: $"namespace {string.Join(".", classSymbol.ContainingNamespace.ConstituentNamespaces)} {{",
        identChange: CodeBuilder.IdentChange.IncreaseAfter
      );
    }

    var containingType = classSymbol.ContainingType;
    var i = 0;
    while (containingType != null) {
      codeBuilder.appendLine($"public partial class {containingType.Name} {{", CodeBuilder.IdentChange.IncreaseAfter);

      containingType = containingType.ContainingType;
      i++;
    }

    codeBuilder.appendLine($"public partial class {classSymbol.Name} {{", CodeBuilder.IdentChange.IncreaseAfter);

    foreach (var fieldSymbol in fields) {
      processField(codeBuilder, fieldSymbol);
    }

    for (var j = 0; j < i; j++) {
      codeBuilder.appendLine("}", CodeBuilder.IdentChange.DecreaseBefore);
    }

    if (!classSymbol.ContainingNamespace.IsGlobalNamespace) {
      codeBuilder.appendLine("}", CodeBuilder.IdentChange.DecreaseBefore);
    }

    codeBuilder.appendLine("}", CodeBuilder.IdentChange.DecreaseBefore);

    return codeBuilder.getResult();

    static void processField(CodeBuilder codeBuilder, IFieldSymbol fieldSymbol) {
      var fieldName = fieldSymbol.Name;
      var fieldType = fieldSymbol.Type;

      codeBuilder.appendEmptyLine();
      codeBuilder.appendLine($"/// Public accessor for <see cref=\"{fieldName}\"/>");
      codeBuilder.appendLine($"/// <inheritdoc cref=\"{fieldName}\"/>");
      codeBuilder.appendLine($"public {fieldType} _{fieldName} => {fieldName};");
    }
  }
}

internal class SyntaxReceiver : ISyntaxContextReceiver {
  public List<IFieldSymbol> fields { get; } = new();

  public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
    if (context.Node is not FieldDeclarationSyntax { AttributeLists.Count: > 0 } fieldDeclarationSyntax) return;
    
    foreach (var variable in fieldDeclarationSyntax.Declaration.Variables) {
      var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
      var haveAttribute =
        fieldSymbol != null
        && fieldSymbol.GetAttributes()
          .Any(ad =>
            ad.AttributeClass != null
            && ad.AttributeClass.ToDisplayString() == PublicAccessorGenerator.AttributeName
          );

      if (haveAttribute) {
        fields.Add(fieldSymbol);
      }
    }
  }
}