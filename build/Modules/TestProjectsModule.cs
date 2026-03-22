using ModularPipelines.Attributes;
using ModularPipelines.Conditions;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Modules;
using Sourcy.DotNet;

namespace Build.Modules;

/// <summary>
///     Test the add-in for each supported Revit configuration.
/// </summary>
[SkipIf<IsCI>]
[DependsOn<ResolveConfigurationsModule>]
[DependsOn<CompileProjectsModule>(Optional = true)]
public sealed class TestProjectsModule : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var configurationsResult = await context.GetModule<ResolveConfigurationsModule>();
        var configurations = configurationsResult.ValueOrDefault!;
        string[] languages =
        [
            "ENU",
            "RUS",
            "CHS"
        ];

        foreach (var configuration in configurations.Where(configuration => !configuration.Contains("27")))
        {
            foreach (var language in languages)
            {
                await context.SubModule($"{configuration}|{language}", async () => await TestAsync(context, configuration, language, cancellationToken));
            }
        }
    }

    /// <summary>
    ///     Test the project for the specified Revit configuration.
    /// </summary>
    private static async Task TestAsync(IModuleContext context, string configuration, string language, CancellationToken cancellationToken)
    {
        await context.DotNet().Test(new DotNetTestOptions
        {
            Project = Projects.Nice3point_TUnit_Revit_Tests.FullName,
            Configuration = configuration,
            Properties =
            [
                ("RevitLanguage", language)
            ]
        }, cancellationToken: cancellationToken);
    }
}