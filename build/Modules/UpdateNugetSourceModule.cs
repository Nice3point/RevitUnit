using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class UpdateNugetSourceModule(IOptions<NuGetOptions> nugetOptions) : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        await context.DotNet().Nuget.Update.Source(new DotNetNugetUpdateSourceOptions("nice3point")
        {
            Username = "Nice3point",
            Password = nugetOptions.Value.InternalApiKey
        }, cancellationToken);

        return await NothingAsync();
    }
}