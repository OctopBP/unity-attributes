using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PublicAccessor;

[Generator]
internal sealed partial class PublicAccessorGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    // File.WriteAllText(
    //   "/Users/b.proshin/Projects/PublicAccessor/PublicAccessor/Test2.txt",
    //   "// Test file 4"
    // );
    
    context.RegisterPostInitializationOutput(PostInitializationOutput);
    
    var provider = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform);
    context.RegisterSourceOutput(provider, Execute);
  }
  
  static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context) =>
    context.AddSource($"{PublicAccessorAttributeName}.g.cs", PublicAccessorAttribute);

  static bool Predicate(SyntaxNode syntaxNode, CancellationToken _) =>
    syntaxNode is FieldDeclarationSyntax {AttributeLists.Count: > 0} field
    && !field.Modifiers.Any(SyntaxKind.PublicKeyword)
    && !field.Modifiers.Any(SyntaxKind.StaticKeyword);

  static (string filename, string classname) Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
    return "Test";
    
    // Debug.Assert(context.Node is MethodDeclarationSyntax);
    // Debug.Assert(context.Node.Parent is TypeDeclarationSyntax);
    //
    // var method = Unsafe.As<MethodDeclarationSyntax>(context.Node);
    // var type = Unsafe.As<TypeDeclarationSyntax>(context.Node.Parent);
    //
    // var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method, cancellationToken);
    // Debug.Assert(methodSymbol is not null);
    //
    // if (!HasAttribute(method, PublicAccessorAttributeName, context.SemanticModel, cancellationToken)) {
    //   return default;
    // }
    //
    // var param = methodSymbol.Parameters.Single();
    // if (methodSymbol.ReturnType.SpecialType == SpecialType.System_String && param.Type.TypeKind == TypeKind.Enum) {
    //   return "";
    //   // return (
    //   //   methodName: method.Identifier.ValueText, modifiers: method.Modifiers.ToString(),
    //   //   enumType: param.Type.ToDisplayString(), paramName: param.Name, keyword: type.Keyword.ToString(),
    //   //   isNullable: methodSymbol.ReturnType.NullableAnnotation == NullableAnnotation.Annotated,
    //   //   constants: param.Type.GetMembers()
    //   //     .Where(static member => member.Kind is SymbolKind.Field)
    //   //     .Select(static member => member.ToDisplayString())
    //   //     .ToImmutableArray(), typeName: methodSymbol.ContainingType.Name,
    //   //   nameSpace: methodSymbol.ContainingNamespace.ToDisplayString(),
    //   //   isGlobal: methodSymbol.ContainingNamespace.IsGlobalNamespace
    //   // );
    // }
    //
    // return default;
  }

  static void Execute(SourceProductionContext context, string filename, string classname) {
    // var value = values.First();
    var hintName = $"{filename}.transformed.g.cs";
      // value.isGlobal
      // ? $"{value.typeName}.PublicAccessor.g.cs"
      // : $"{value.nameSpace}.{value.typeName}.PublicAccessor.g.cs";
      
    Console.WriteLine($"Execute {hintName}");

    //
    // File.WriteAllText(
    //   "/Users/b.proshin/Projects/PublicAccessor/PublicAccessor/Test.PublicAccessorGenerator.cs",
    //   "// Test file"
    // );


    context.AddSource(hintName, @$"// Test file
public partial class {classname} {{

public void Test() {{ }}

}}
");
  }
}