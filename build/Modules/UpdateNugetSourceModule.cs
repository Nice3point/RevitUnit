using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class UpdateNugetSourceModule : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        await context.DotNet().Nuget.Update.Source(new DotNetNugetUpdateSourceOptions("nice3point")
        {
            Username = context.GitHub().EnvironmentVariables.Actor,
            Password = context.GitHub().EnvironmentVariables.Token
        }, cancellationToken);

        return await NothingAsync();
    }
}