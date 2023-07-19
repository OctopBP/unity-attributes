using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityAttributes.Readonly;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReadonlyAnalyzer : DiagnosticAnalyzer {
    const string DiagnosticId = "ReadonlyAnalyzer";

    static readonly DiagnosticDescriptor AssignmentRule = new DiagnosticDescriptor(
        DiagnosticId,
        title: "Field assignment detected",
        messageFormat: "Field '{0}' can be assigned.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assignment to fields should be avoided"
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register a semantic model action to analyze the assignments
        context.RegisterSemanticModelAction(AnalyzeAssignment);
    }

    static void AnalyzeAssignment(SemanticModelAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;
        var root = semanticModel.SyntaxTree.GetRoot(context.CancellationToken);

        // Find all assignments in the syntax tree
        var assignmentExpressions = root.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>();

        foreach (var assignmentExpression in assignmentExpressions)
        {
            // Get the assigned symbol
            var symbol = semanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;
            
            // Check if the symbol is a property
            if (symbol is IFieldSymbol property && property.GetAttributes().Any(attr => 
                    attr.AttributeClass != null
                    && attr.AttributeClass.ToDisplayString() == ReadonlyGenerator.AttributeName
            )) {
                // Report a diagnostic for each assignment to property
                var diagnostic = Diagnostic.Create(AssignmentRule, assignmentExpression.GetLocation(), assignmentExpression.Left.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssignmentRule);
}
