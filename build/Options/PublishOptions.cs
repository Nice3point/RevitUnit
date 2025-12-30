namespace Build.Options;

[Serializable]
public sealed record PublishOptions
{
    public string? ChangelogFile { get; init; }
}