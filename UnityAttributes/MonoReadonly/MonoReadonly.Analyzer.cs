using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityAttributes.MonoReadonly;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MonoReadonlyAnalyzer : DiagnosticAnalyzer {
    static readonly string[] AllowedMethodsNames = { "Awake", "OnEnable", "Start", "Reset" };

    const string DiagnosticId = "MonoReadonlyAnalyzer";

    static readonly DiagnosticDescriptor AssignmentRule = new DiagnosticDescriptor(
        DiagnosticId,
        title: "Field assignment detected",
        messageFormat: $"Field '{{0}}' can be assigned only in {string.Join(", ", AllowedMethodsNames)} methods.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assignment to fields should be avoided"
    );
    
    static readonly DiagnosticDescriptor DuplicateAssignmentRule = new DiagnosticDescriptor(
        DiagnosticId,
        title: "Duplicate field assignment detected",
        messageFormat: "Field '{0}' assigned more than in one place.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Assignment duplication to fields should be avoided"
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

        var propertiesNames = new List<string>();
        
        foreach (var assignmentExpression in assignmentExpressions)
        {
            // Get the assigned symbol
            var symbol = semanticModel.GetSymbolInfo(assignmentExpression.Left).Symbol;
            
            // Check if the symbol is a property
            if (symbol is IFieldSymbol property && property.GetAttributes().Any(attr => 
                    attr.AttributeClass != null
                    && attr.AttributeClass.ToDisplayString() == MonoReadonlyGenerator.AttributeName
            )) {
                if (propertiesNames.Contains(property.Name))
                {
                    var diagnostic = Diagnostic.Create(DuplicateAssignmentRule, assignmentExpression.GetLocation(), assignmentExpression.Left.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    propertiesNames.Add(property.Name);
                }
                
                // Report a diagnostic for each assignment to property
                var methodDeclaration = assignmentExpression.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (methodDeclaration != null && !AllowedMethodsNames.Contains(methodDeclaration.Identifier.ValueText))
                {
                    var diagnostic = Diagnostic.Create(AssignmentRule, assignmentExpression.GetLocation(), assignmentExpression.Left.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AssignmentRule, DuplicateAssignmentRule);
}
