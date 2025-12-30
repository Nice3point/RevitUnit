using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Shouldly;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Compile the project.
/// </summary>
[DependsOn<UpdateNugetSourceModule>]
[DependsOn<ResolveConfigurationsModule>]
public sealed class CompileProjectModule(IOptions<BuildOptions> buildOptions) : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var configurationsResult = await GetModule<ResolveConfigurationsModule>();
        var configurations = configurationsResult.Value!;

        foreach (var configuration in configurations)
        {
            await SubModule(configuration, async () => await CompileAsync(context, configuration, cancellationToken));
        }

        return await NothingAsync();
    }

    private async Task<CommandResult> CompileAsync(IPipelineContext context, string configuration, CancellationToken cancellationToken)
    {
        buildOptions.Value.Versions
            .TryGetValue(configuration, out var version)
            .ShouldBeTrue($"Can't find pack version for configuration: {configuration}");

        return await context.DotNet().Build(new DotNetBuildOptions
        {
            ProjectSolution = Projects.Nice3point_TUnit_Revit.FullName,
            Configuration = configuration,
            Verbosity = Verbosity.Minimal,
            Properties = new List<KeyValue>
            {
                ("Version", version)
            }
        }, cancellationToken);
    }
}