namespace UnityAttributes.ShaderProperty;

internal sealed record PropertyToProcess(string Name, string Type, string Mode)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public string Mode { get; } = Mode;
}


