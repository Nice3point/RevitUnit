sealed partial class Build
{
    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output";
    AbsolutePath PrivateArtifactsDirectory => ArtifactsDirectory / "private";
    AbsolutePath PublicArtifactsDirectory => ArtifactsDirectory / "public";

    [Parameter] string ReleaseVersion;

    protected override void OnBuildInitialized()
    {
        ReleaseVersion ??= GitRepository.Tags.SingleOrDefault();

        Configurations =
        [
            "Release*"
        ];

        PackageVersionsMap = new()
        {
            { "Release R22", "2022.0.1" },
            { "Release R23", "2023.0.1" },
            { "Release R24", "2024.0.1" },
            { "Release R25", "2025.0.1" },
            { "Release R26", "2026.0.1" }
        };
    }
}