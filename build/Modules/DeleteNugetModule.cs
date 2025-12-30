using Build.Options;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class DeleteNugetModule(IOptions<BuildOptions> buildOptions, IOptions<NuGetOptions> nugetOptions) : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        await buildOptions.Value.Versions.Values
            .ForEachAsync(async version => { await DeleteNugetPackageAsync(context, version, nugetOptions.Value.Source, nugetOptions.Value.ApiKey, cancellationToken); },
                cancellationToken)
            .ProcessOneAtATime();

        return await NothingAsync();
    }

    private async Task<CommandResult> DeleteNugetPackageAsync(IPipelineContext context, string version, string source, string apiKey, CancellationToken cancellationToken)
    {
        return await context.DotNet().Nuget.Delete(new DotNetNugetDeleteOptions
        {
            PackageName = "Nice3point.BenchmarkDotNet.Revit",
            PackageVersion = version,
            ApiKey = apiKey,
            Source = source,
            NonInteractive = true
        }, cancellationToken);
    }
}