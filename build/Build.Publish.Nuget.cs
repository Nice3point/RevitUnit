using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    const string NugetPublicSource = "https://api.nuget.org/v3/index.json";
    const string NugetPrivateSource = "https://nuget.pkg.github.com/Nice3point/index.json";

    [Parameter] [Secret] string NugetPublicApiKey = EnvironmentInfo.GetVariable("NUGET_API_KEY");
    [Parameter] [Secret] string NugetPrivateApiKey = EnvironmentInfo.GetVariable("NICE3POINT_PACKAGES_API_KEY");

    Target PublishNuget => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetPublicApiKey)
        .Requires(() => NugetPrivateApiKey)
        .Executes(() =>
        {
            // foreach (var package in PrivateArtifactsDirectory.GlobFiles("*.nupkg"))
            // {
            //     DotNetNuGetPush(settings => settings
            //         .SetTargetPath(package)
            //         .SetSource(NugetPrivateSource)
            //         .SetApiKey(NugetPrivateApiKey));
            // }

            foreach (var package in PublicArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                var fileInfo = package.ToFileInfo();
                Assert.False(fileInfo.Length > 40 * 1024, "file length > 40 kb, check assembly trimming. Public distribution of source code should be avoided");

                DotNetNuGetPush(settings => settings
                    .SetTargetPath(package)
                    .SetSource(NugetPublicSource)
                    .SetApiKey(NugetPublicApiKey));
            }
        });
}