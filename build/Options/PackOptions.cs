using System.ComponentModel.DataAnnotations;

namespace Build.Options;

[Serializable]
public sealed record PackOptions
{
    [Required] public string OutputDirectory { get; init; } = null!;
    [Required] public string PrivateOutputDirectory { get; init; } = null!;
    [Required] public string PublicOutputDirectory { get; init; } = null!;
}