using System;
using Microsoft.CodeAnalysis;

namespace SourceGeneration.Utils.CodeBuilder;

public class ParentsBlock : IDisposable
{
    private readonly CodeBuilder _builder;
    private readonly bool _exist;

    public ParentsBlock(CodeBuilder builder, ITypeSymbol typeSymbol)
    {
        _builder = builder;
        var containingType = typeSymbol.ContainingType;
        if (containingType != null)
        {
            _exist = true;
            using (new ParentsBlock(builder, containingType))
            {
                builder.AppendIdent().Append("public partial class ").Append(containingType.Name).AppendLine();
                _builder.OpenBrackets();
            }
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