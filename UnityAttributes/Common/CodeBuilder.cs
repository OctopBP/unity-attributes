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
  
  const string Space = "  ";
  
  readonly StringBuilder stringBuilder = new();
  int ident;

  public void append(string text) => stringBuilder.Append(text);
  public void appendEmptyLine() => stringBuilder.AppendLine();

  public void appendLine(string text, IdentChange identChange = IdentChange.None) {
    switch (identChange) {
      case IdentChange.None:
      case IdentChange.IncreaseAfter:
      case IdentChange.DecreaseAfter:
        break;
      case IdentChange.IncreaseBefore:
        ident++; break;
      case IdentChange.DecreaseBefore:
        ident--; break;
      default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
    }
    
    stringBuilder.AppendLine(identToSpaces + text);
    
    switch (identChange) {
      case IdentChange.None:
      case IdentChange.IncreaseBefore:
      case IdentChange.DecreaseBefore:
        break;
      case IdentChange.IncreaseAfter:
        ident++; break;
      case IdentChange.DecreaseAfter:
        ident--; break;
      default: throw new ArgumentOutOfRangeException(nameof(identChange), identChange, null);
    }
  
    string identToSpaces() {
      if (ident <= 0) return string.Empty;
      var textAsSpan = Space.AsSpan();
      var span = new Span<char>(new char[textAsSpan.Length * ident]);
      for (var idx = 0; idx < ident; idx++) {
        textAsSpan.CopyTo(span.Slice(idx * textAsSpan.Length, textAsSpan.Length));
      }

      return span.ToString();
    }
  }

  public string getResult() {
    return stringBuilder.ToString();
  }
}