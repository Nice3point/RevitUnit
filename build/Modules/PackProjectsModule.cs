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

[DependsOn<CleanProjectsModule>]
[DependsOn<UpdateNugetSourceModule>]
[DependsOn<ParseSolutionConfigurationsModule>]
public sealed class PackProjectsModule(IOptions<BuildOptions> buildOptions, IOptions<PackOptions> packOptions) : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var configurations = await GetModule<ParseSolutionConfigurationsModule>();
        var privateOutputFolder = context.Git().RootDirectory
            .GetFolder(packOptions.Value.OutputDirectory)
            .GetFolder(packOptions.Value.PrivateOutputDirectory);
        var publicOutputFolder = context.Git().RootDirectory
            .GetFolder(packOptions.Value.OutputDirectory)
            .GetFolder(packOptions.Value.PublicOutputDirectory);

        foreach (var configuration in configurations.Value!)
        {
            await SubModule(configuration, async () =>
            {
                await PackPrivateAsync(context, configuration, privateOutputFolder.Path, cancellationToken);
                await PackPublicAsync(context, configuration, publicOutputFolder.Path, cancellationToken);
            });
        }

        return await NothingAsync();
    }

    private async Task<CommandResult> PackPrivateAsync(IPipelineContext context, string configuration, string output, CancellationToken cancellationToken)
    {
        buildOptions.Value.Versions
            .TryGetValue(configuration, out var version)
            .ShouldBeTrue($"Can't find pack version for configuration: {configuration}");

        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = Projects.Nice3point_TUnit_Revit.FullName,
            Configuration = configuration,
            Verbosity = Verbosity.Minimal,
            NoBuild = true,
            Properties = new List<KeyValue>
            {
                ("Version", version.ToString()),
                ("PublishProfile", "Private"),
            },
            OutputDirectory = output
        }, cancellationToken);
    }

    private async Task<CommandResult> PackPublicAsync(IPipelineContext context, string configuration, string output, CancellationToken cancellationToken)
    {
        buildOptions.Value.Versions
            .TryGetValue(configuration, out var version)
            .ShouldBeTrue($"Can't find pack version for configuration: {configuration}");

        return await context.DotNet().Pack(new DotNetPackOptions
        {
            ProjectSolution = Projects.Nice3point_TUnit_Revit.FullName,
            Configuration = configuration,
            Verbosity = Verbosity.Minimal,
            NoBuild = true,
            Properties = new List<KeyValue>
            {
                ("Version", version.ToString()),
                ("PublishProfile", "Public"),
                ("ProduceOnlyReferenceAssembly", "true")
            },
            OutputDirectory = output
        }, cancellationToken);
    }
}