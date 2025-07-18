using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Pack => _ => _
        .DependsOn(Compile)
        .Requires(() => ReleaseVersion)
        .Executes(() =>
        {
            foreach (var configuration in GlobBuildConfigurations())
            {
                DotNetPack(settings => settings
                    .SetConfiguration(configuration)
                    .SetVersion(GetPackVersion(configuration))
                    .SetOutputDirectory(ArtifactsDirectory)
                    .SetVerbosity(DotNetVerbosity.minimal));
            }
        });

    string GetPackVersion(string configuration)
    {
        if (PackageVersionsMap.TryGetValue(configuration, out var version)) return version;
        throw new Exception($"Can't find pack version for configuration: {configuration}");
    }
}