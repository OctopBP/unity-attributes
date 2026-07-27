namespace UnityAttributes.ShaderProperty;

internal sealed record PropertyToProcess(string Name, string Type, string Mode, bool IsArray = false, int Count = 1, int StartIndex = 1)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public string Mode { get; } = Mode;
    public bool IsArray { get; } = IsArray;
    public int Count { get; } = Count;
    public int StartIndex { get; } = StartIndex;
}
