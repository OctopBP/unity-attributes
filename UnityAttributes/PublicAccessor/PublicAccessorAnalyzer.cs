using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityAttributes.PublicAccessor; 

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PublicAccessorAnalyzer : DiagnosticAnalyzer {
  const string DiagnosticId = "PublicAccessorAttributeAnalyzer";
  const string Category = "InitializationSafety";
  static readonly LocalizableString title = "InitializeComponents method should be called";
  static readonly LocalizableString messageFormat = "InitializeComponents method should be called";
  static readonly LocalizableString description = "InitializeComponents method should be called";
  const string HelpLinkUri = "";

  static readonly DiagnosticDescriptor rule = new(
    DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Warning,
    isEnabledByDefault: true, description: description, helpLinkUri: HelpLinkUri
  );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

  public override void Initialize(AnalysisContext context) {
    context.EnableConcurrentExecution();
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.RegisterSyntaxNodeAction(analyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
  }

  static void analyzeFieldDeclaration(SyntaxNodeAnalysisContext context) {
    return;
    
    var fieldDeclarationSyntax = (FieldDeclarationSyntax)context.Node;
    var fieldSymbol = (IFieldSymbol)context.ContainingSymbol;

    if (!hasPublicAccessorAttribute(fieldSymbol)) return;

    var classNode = fieldSymbol.ContainingType.DeclaringSyntaxReferences.FirstOrDefault()
      ?.GetSyntax();

    foreach (var expressionSyntax in classNode.DescendantNodes().OfType<InvocationExpressionSyntax>()) {
      if (context.SemanticModel.GetSymbolInfo(expressionSyntax).Symbol is not IMethodSymbol methodSymbol) continue;
      if (methodSymbol.Name == "InitializeComponents")
        return;
    }

    // var diagnostic = Diagnostic.Create(rule, fieldDeclarationSyntax.GetLocation());
    // context.ReportDiagnostic(diagnostic);
  }

  static bool hasPublicAccessorAttribute(ISymbol fieldSymbol) => fieldSymbol.GetAttributes()
    .Any(ad => ad?.AttributeClass?.ToDisplayString() == PublicAccessorGenerator.AttributeName);
}