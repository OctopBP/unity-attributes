using System;

namespace SourceGeneration.Utils.CodeBuilder;

public class BracketsBlock : IDisposable
{
    private readonly CodeBuilder _builder;

    public BracketsBlock(CodeBuilder builder)
    {
        _builder = builder;
        _builder.OpenBrackets();
    }

    public void Dispose()
    {
        _builder.CloseBrackets();
    }
}