using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class UpdateNugetSourceModule(IOptions<NuGetOptions> nugetOptions) : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        await context.DotNet().Nuget.Update.Source(new DotNetNugetUpdateSourceOptions
        {
            Username = "Nice3point",
            Password = nugetOptions.Value.InternalApiKey,
            Arguments = ["nice3point"]
        }, cancellationToken: cancellationToken);
    }
}