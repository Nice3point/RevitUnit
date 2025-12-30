using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace Build.Options;

[Serializable]
public sealed record NuGetOptions
{
    [Required] [SecretValue] public string InternalApiKey { get; init; } = null!;
    [SecretValue] public string ApiKey { get; init; } = null!;
    public string Source { get; init; } = null!;
}