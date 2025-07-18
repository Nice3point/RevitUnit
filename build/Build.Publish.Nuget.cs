using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    const string NugetSource = "https://nuget.pkg.github.com/Nice3point/index.json";
    [Parameter] [Secret] string NugetApiKey = EnvironmentInfo.GetVariable("NICE3POINT_PACKAGES_API_KEY");

    Target PublishNuget => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiKey)
        .Executes(() =>
        {
            foreach (var package in ArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                DotNetNuGetPush(settings => settings
                    .SetTargetPath(package)
                    .SetSource(NugetSource)
                    .SetApiKey(NugetApiKey));
            }
        });
}