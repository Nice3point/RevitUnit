using System.ComponentModel.DataAnnotations;
using ModularPipelines.Attributes;

namespace Build.Options;

[Serializable]
public sealed record NuGetOptions
{
    [Required] [SecretValue] public string PrivateApiKey { get; init; } = null!;
    [Required] [SecretValue] public string PublicApiKey { get; init; } = null!;
    [Required] public string PrivateSource { get; init; } = null!;
    [Required] public string PublicSource { get; init; } = null!;
}