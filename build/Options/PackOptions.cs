namespace Build.Options;

[Serializable]
public sealed record PackOptions
{
    public string? Version { get; init; }
}