using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularPipelines.Context;
using ModularPipelines.Engine;

namespace Build.ILRepack;

public static class IlRepackExtensions
{
    [ModuleInitializer]
    public static void RegisterIlRepackContext()
    {
        ModularPipelinesContextRegistry.RegisterContext(collection => collection.RegisterIlRepackContext());
    }

    private static IServiceCollection RegisterIlRepackContext(this IServiceCollection services)
    {
        services.TryAddScoped<ILRepack>();
        return services;
    }

    public static ILRepack IlRepack(this IPipelineHookContext context) => context.ServiceProvider.GetRequiredService<ILRepack>();
}