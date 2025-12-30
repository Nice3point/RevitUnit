using Build.ILRepack.Options;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Models;
using ModularPipelines.Options;

namespace Build.ILRepack;

public sealed class ILRepack(IPipelineContext context)
{
    private readonly Folder _temporaryFolder = context.FileSystem.CreateTemporaryFolder();
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public async Task<CommandResult> Repack(IlRepackOptions options, CancellationToken cancellationToken = default)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            await context.Command.ExecuteCommandLineTool(new CommandLineToolOptions("dotnet")
            {
                Arguments =
                [
                    "tool",
                    "install",
                    "--tool-path", _temporaryFolder.Path,
                    "dotnet-ilrepack"
                ]
            }, cancellationToken);
            // await context.DotNet().Tool.Install(new DotNetToolInstallOptions("dotnet-ilrepack")
            // {
            //     ToolPath = _temporaryFolder.Path
            // }, cancellationToken);

            return await context.Command.ExecuteCommandLineTool(options with
            {
                Tool = Path.Combine(_temporaryFolder.Path, options.Tool)
            }, cancellationToken);
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }
}