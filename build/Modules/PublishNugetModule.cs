using Build.Options;
using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using Shouldly;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

[DependsOn<PackProjectsModule>]
public sealed class PublishNugetModule(IOptions<PackOptions> packOptions, IOptions<NuGetOptions> nuGetOptions) : Module<CommandResult[]?>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var privateOutputFolder = context.Git().RootDirectory.GetFolder(packOptions.Value.OutputDirectory);
        var targetPackages = privateOutputFolder.GetFiles(file => file.Extension == ".nupkg").ToArray();
        targetPackages.ShouldNotBeEmpty("No NuGet packages were found to publish");

        await targetPackages
            .SelectAsync(async file => await PushAsync(context, file, nuGetOptions.Value.Source, nuGetOptions.Value.ApiKey, cancellationToken), cancellationToken)
            .ProcessInParallel();

        return await NothingAsync();
    }

    private Task<CommandResult> PushAsync(IPipelineContext context, File file, string source, string apiKey, CancellationToken cancellationToken)
    {
        return context.DotNet().Nuget.Push(new DotNetNugetPushOptions
        {
            Path = file,
            ApiKey = apiKey,
            Source = source
        }, cancellationToken);
    }
}