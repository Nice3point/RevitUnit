using Build.Modules;
using Build.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines.Extensions;
using ModularPipelines.Host;

await PipelineHostBuilder.Create()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, collection) =>
    {
        collection.AddOptions<BuildOptions>().Bind(context.Configuration.GetSection("Build")).ValidateDataAnnotations();
        collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();

        if (args.Contains("delete-nuget"))
        {
            collection.AddOptions<NuGetOptions>().Bind(context.Configuration.GetSection("NuGet")).ValidateDataAnnotations();
            collection.AddModule<DeleteNugetModule>();
            return;
        }

        collection.AddModule<CleanProjectModule>();
        collection.AddModule<CompileProjectModule>();
        collection.AddModule<ResolveConfigurationsModule>();
        collection.AddModule<UpdateNugetSourceModule>();

        if (args.Contains("pack"))
        {
            collection.AddOptions<PackOptions>().Bind(context.Configuration.GetSection("Pack")).ValidateDataAnnotations();

            collection.AddModule<ResolvePackVersionModule>();
            collection.AddModule<RepackInjectorModule>();
            collection.AddModule<PackNugetModule>();
            collection.AddModule<GenerateChangelogModule>();
            collection.AddModule<GenerateNugetChangelogModule>();
            collection.AddModule<UpdateReadmeModule>();
            collection.AddModule<RestoreReadmeModule>();
        }

        if (args.Contains("publish"))
        {
            collection.AddOptions<PublishOptions>().Bind(context.Configuration.GetSection("Publish")).ValidateDataAnnotations();

            collection.AddModule<GenerateGitHubChangelogModule>();
            collection.AddModule<PublishNugetModule>();
            collection.AddModule<PublishGithubModule>();
        }
    })
    .ExecutePipelineAsync();