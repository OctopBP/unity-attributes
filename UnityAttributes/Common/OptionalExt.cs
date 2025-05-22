using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceGeneration.Utils.Common;

public static class OptionalExt
{
    public static Optional<T> None<T>()
    {
        return new Optional<T>();
    }
    
    public static Optional<T> Some<T>(T value)
    {
        return new Optional<T>(value);
    }

    public static IEnumerable<T> Collect<T>(this IEnumerable<Optional<T>> self)
    {
        return self.Where(o => o.HasValue).Select(o => o.Value);
    }
}