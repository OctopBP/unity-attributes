using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace UnityAttributes.Test;

public static class TestHelper
{
    public static Task Verify<T>(string source, string directory) where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(assemblyName: "Tests", syntaxTrees: new[] { syntaxTree });
        
        var generator = new T();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);
        
        return Verifier.Verify(driver, new VerifySettings()).UseDirectory(directory);
    }
}