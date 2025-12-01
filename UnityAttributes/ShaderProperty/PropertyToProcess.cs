namespace UnityAttributes.ShaderProperty;

internal sealed record PropertyToProcess(string Name, string Type, bool IsGlobal)
{
    public string Name { get; } = Name;
    public string Type { get; } = Type;
    public bool IsGlobal { get; } = IsGlobal;
}


