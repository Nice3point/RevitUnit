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
[DependsOn<CleanProjectsModule>(Optional = true)]
public sealed class CompileProjectsModule(IOptions<BuildOptions> buildOptions) : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var configurationsResult = await context.GetModule<ResolveConfigurationsModule>();
        var configurations = configurationsResult.ValueOrDefault!;

        foreach (var configuration in configurations)
        {
            await context.SubModule(configuration, async () => await CompileAsync(context, configuration, cancellationToken));
        }
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
            Properties = new List<KeyValue>
            {
                ("Version", version)
            }
        }, cancellationToken: cancellationToken);
    }
}