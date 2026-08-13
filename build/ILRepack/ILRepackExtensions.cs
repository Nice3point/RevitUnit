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

    extension(IServiceCollection services)
    {
        private IServiceCollection RegisterIlRepackContext()
        {
            services.TryAddScoped<ILRepack>();
            return services;
        }
    }

    extension(IPipelineContext context)
    {
        public ILRepack IlRepack()
        {
            return context.Services.Get<ILRepack>();
        }
    }
}
