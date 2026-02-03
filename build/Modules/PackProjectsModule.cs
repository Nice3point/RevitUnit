using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Shouldly;
using Sourcy.DotNet;

namespace Build.Modules;

[DependsOn<RepackInjectorModule>]
[DependsOn<GenerateNugetChangelogModule>]
[DependsOn<ResolveConfigurationsModule>]
public sealed class PackProjectsModule(IOptions<BuildOptions> buildOptions) : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var configurationsResult = await context.GetModule<ResolveConfigurationsModule>();
        var changelogModule = context.GetModuleIfRegistered<GenerateNugetChangelogModule>();

        var configurations = configurationsResult.ValueOrDefault!;
        var changelogResult = changelogModule is null ? null : await changelogModule;
        var changelog = changelogResult?.ValueOrDefault;
        var outputFolder = context.Git().RootDirectory.GetFolder(buildOptions.Value.OutputDirectory);

        foreach (var configuration in configurations)
        {
            await context.SubModule(configuration, async () => await PackAsync(
                context,
                configuration,
                changelog,
                outputFolder.Path,
                cancellationToken));
        }
    }

    private async Task<CommandResult> PackAsync(IModuleContext context,
        string configuration,
        string? changelog,
        string output,
        CancellationToken cancellationToken)
    {
        buildOptions.Value.Versions
            .TryGetValue(configuration, out var version)
            .ShouldBeTrue($"Can't find pack version for configuration: {configuration}");

        await context.DotNet().Restore(new DotNetRestoreOptions
        {
            ProjectSolution = Projects.Nice3point_TUnit_Revit.FullName,
            Properties = new List<KeyValue>
            {
                ("Configuration", configuration)
            }
        }, cancellationToken: cancellationToken);

        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = Projects.Nice3point_TUnit_Revit.FullName,
            Configuration = configuration,
            NoBuild = true,
            Properties =
            [
                ("Version", version),
                ("PackageReleaseNotes", changelog ?? string.Empty)
            ],
            Output = output
        }, cancellationToken: cancellationToken);
    }
}