using Microsoft.CodeAnalysis;

namespace UnityAttributes.PublicAccessor;

internal sealed record FieldToProcess(IFieldSymbol FieldSymbol)
{
    public IFieldSymbol FieldSymbol { get; } = FieldSymbol;
}