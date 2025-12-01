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
        var privateOutputFolder = context.Git().RootDirectory
            .GetFolder(packOptions.Value.OutputDirectory)
            .GetFolder(packOptions.Value.PrivateOutputDirectory);
        var publicOutputFolder = context.Git().RootDirectory
            .GetFolder(packOptions.Value.OutputDirectory)
            .GetFolder(packOptions.Value.PublicOutputDirectory);

        var privateTargetPackages = privateOutputFolder.GetFiles(file => file.Extension == ".nupkg").ToArray();
        // var publicTargetPackages = publicOutputFolder.GetFiles(file => file.Extension == ".nupkg").ToArray();
        privateTargetPackages.ShouldNotBeEmpty("No NuGet packages were found to publish");
        // publicTargetPackages.ShouldNotBeEmpty("No NuGet packages were found to publish");

        await privateTargetPackages
            .SelectAsync(async file => await PushAsync(context, file, nuGetOptions.Value.PrivateSource, nuGetOptions.Value.PrivateApiKey, cancellationToken), cancellationToken)
            .ProcessInParallel();

        // await publicTargetPackages
        //     .SelectAsync(async file =>
        //     {
        //         file.Length.ShouldBeLessThan(40 * 1024, "File length > 40 kb, check assembly trimming. Public distribution of source code should be avoided");
        //
        //         return await PushAsync(context, file, nuGetOptions.Value.PublicSource, nuGetOptions.Value.PublicApiKey, cancellationToken);
        //     }, cancellationToken)
        //     .ProcessInParallel();

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