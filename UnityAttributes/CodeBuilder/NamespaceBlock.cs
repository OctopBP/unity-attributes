using System;
using Microsoft.CodeAnalysis;
using SourceGeneration.Utils.CodeAnalysisExtensions;

namespace SourceGeneration.Utils.CodeBuilder;

public class NamespaceBlock : IDisposable
{
    private readonly CodeBuilder _builder;
    private readonly bool _exist;

    public NamespaceBlock(CodeBuilder builder, ISymbol symbol)
    {
        _builder = builder;
            
        var @namespace = symbol.GetNamespace();
        _exist = !string.IsNullOrEmpty(@namespace);
            
        if (_exist)
        {
            builder.AppendIdent().Append("namespace ").Append(@namespace).AppendLine();
            _builder.OpenBrackets();
        }
    }
    
    public NamespaceBlock(CodeBuilder builder, string @namespace)
    {
        _builder = builder;
        
        _exist = !string.IsNullOrEmpty(@namespace);
            
        if (_exist)
        {
            builder.AppendIdent().Append("namespace ").Append(@namespace).AppendLine();
            _builder.OpenBrackets();
        }
    }

    public void Dispose()
    {
        if (_exist)
        {
            _builder.CloseBrackets();
        }
    }
}