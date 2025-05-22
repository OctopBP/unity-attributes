using System;

namespace SourceGeneration.Utils.CodeBuilder;

public class BracketsBlock : IDisposable
{
    private readonly CodeBuilder _builder;
    private readonly bool _withSemicolon;

    public BracketsBlock(CodeBuilder builder, bool withSemicolon = false)
    {
        _builder = builder;
        _withSemicolon = withSemicolon;
        _builder.OpenBrackets();
    }

    public void Dispose()
    {
        if (_withSemicolon)
        {
            _builder.DecreaseIdent().AppendLineWithIdent("};");
        }
        else
        {
            _builder.CloseBrackets();
        }
    }
}