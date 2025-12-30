using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace Build.Modules;

/// <summary>
///     Restore the template Readme.md file after packaging.
/// </summary>
[DependsOn<PackNugetModule>]
[DependsOn<UpdateReadmeModule>]
public sealed class RestoreReadmeModule : Module
{
    public override ModuleRunType ModuleRunType => ModuleRunType.AlwaysRun;

    protected override async Task<bool> ShouldIgnoreFailures(IPipelineContext context, Exception exception)
    {
        var nugetReadmeModule = await GetModule<UpdateReadmeModule>();
        return nugetReadmeModule.HasValue;
    }

    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var nugetReadmeResult = await GetModule<UpdateReadmeModule>();
        if (!nugetReadmeResult.HasValue)
        {
            return await NothingAsync();
        }

        var readmePath = context.Git().RootDirectory.GetFile("Readme.md");
        await readmePath.WriteAsync(nugetReadmeResult.Value!, cancellationToken);
        return await NothingAsync();
    }
}