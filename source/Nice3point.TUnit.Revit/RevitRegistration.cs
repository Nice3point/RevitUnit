using System.ComponentModel;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions.TestHost;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Provides extension methods for <see cref="ITestApplicationBuilder"/> to add the Revit execution model.
/// </summary>
public static class RevitRegistration
{
    /// <param name="builder">The <see cref="ITestApplicationBuilder"/> to add the extensions to.</param>
    extension(ITestApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds the Revit connection lifetime to the specified <see cref="ITestApplicationBuilder"/>.
        /// </summary>
        /// <remarks>
        ///     The build props of the package add this extension to a test project.
        ///     A project that defines its own entry point calls this method from that entry point.
        /// </remarks>
        public void AddRevit()
        {
            builder.TestHost.AddTestHostApplicationLifetime(_ => new RevitConnectionLifetime());
        }
    }
}

/// <summary>
///     Provides the hook Microsoft.Testing.Platform calls while it builds the test application.
/// </summary>
/// <remarks>
///     The <c>TestingPlatformBuilderHook</c> item of the package's build props names this type, and the build step of the test platform compiles a call to <see cref="AddExtensions"/> into the generated entry point.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TestingPlatformBuilderHook
{
    /// <summary>
    ///     Adds the Revit extensions to the specified <see cref="ITestApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ITestApplicationBuilder"/> to add the extensions to.</param>
    /// <param name="arguments">The command line arguments the test application was started with.</param>
    public static void AddExtensions(ITestApplicationBuilder builder, string[] arguments)
    {
        builder.AddRevit();
    }
}
