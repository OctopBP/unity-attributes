using System;
using System.Text;

namespace SourceGeneration.Utils.CodeBuilder;

public class CodeBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private int _ident = 0;

    public CodeBuilder AppendLine()
    {
        _sb.AppendLine();
        return this;
    }
    
    public CodeBuilder AppendLine(string value)
    {
        _sb.AppendLine(value);
        return this;
    }
    
    public CodeBuilder AppendLineWithIdent(string value)
    {
        return AppendIdent().AppendLine(value);
    }
    
    public CodeBuilder Append(string value)
    {
        _sb.Append(value);
        return this;
    }
    
    public CodeBuilder Append(char value)
    {
        _sb.Append(value);
        return this;
    }

    public CodeBuilder AppendIdent()
    {
        for (var i = 0; i < _ident; i++)
        {
            _sb.Append("    ");
        }
        return this;
    }
    
    public CodeBuilder AppendArray<T>(T[] array, Action<T, CodeBuilder> func, Action<CodeBuilder> splitter)
    {
        for(var i = 0; i < array.Length; i++) 
        {
            if (i != 0)
            {
                splitter(this);
            }
            
            func(array[i], this);
        }
        
        return this;
    }

    public CodeBuilder OpenBrackets()
    {
        return AppendLineWithIdent("{").IncreaseIdent();
    }
    
    public CodeBuilder CloseBrackets()
    {
        return DecreaseIdent().AppendLineWithIdent("}");
    }
    
    public CodeBuilder IncreaseIdent()
    {
        _ident++;
        return this;
    }
    
    public CodeBuilder DecreaseIdent()
    {
        if (_ident > 0)
        {
            _ident--;
        }
        return this;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}