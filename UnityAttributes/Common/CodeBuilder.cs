using System;
using System.Text;

namespace UnityAttributes.Common; 

public class CodeBuilder {
  public enum IdentChange {
    None,
    IncreaseBefore,
    DecreaseBefore,
    IncreaseAfter,
    DecreaseAfter
  }
  
  const string IndentSymbol = "  ";
  
  readonly StringBuilder stringBuilder = new();
  int indent;

  public void append(string text) => stringBuilder.Append(text);
  public void appendEmptyLine() => stringBuilder.AppendLine();

  public void appendLine(string text, IdentChange identChange = IdentChange.None) {
    switch (identChange) {
      case IdentChange.None:
      case IdentChange.IncreaseAfter:
      case IdentChange.DecreaseAfter:
        break;
      case IdentChange.IncreaseBefore:
        indent++; break;
      case IdentChange.DecreaseBefore:
        indent--; break;
      default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
    }
    
    stringBuilder.AppendLine(identToSpaces() + text);
    
    switch (identChange) {
      case IdentChange.None:
      case IdentChange.IncreaseBefore:
      case IdentChange.DecreaseBefore:
        break;
      case IdentChange.IncreaseAfter:
        indent++; break;
      case IdentChange.DecreaseAfter:
        indent--; break;
      default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
    }
  
    string identToSpaces() {
      if (indent <= 0) return string.Empty;
      var textAsSpan = IndentSymbol.AsSpan();
      var span = new Span<char>(new char[textAsSpan.Length * indent]);
      for (var idx = 0; idx < indent; idx++) {
        textAsSpan.CopyTo(span.Slice(idx * textAsSpan.Length, textAsSpan.Length));
      }

      return span.ToString();
    }
  }

  public string getResult() {
    return stringBuilder.ToString();
  }
}