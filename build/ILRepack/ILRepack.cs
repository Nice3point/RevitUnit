using Build.ILRepack.Options;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Models;
using ModularPipelines.Options;

namespace Build.ILRepack;

public sealed class ILRepack(IFileSystemContext fileSystem, ICommand command)
{
    private readonly Folder _temporaryFolder = fileSystem.CreateTemporaryFolder();
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    public async Task<CommandResult> Repack(IlRepackOptions options, CancellationToken cancellationToken = default)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            await command.ExecuteCommandLineTool(new CommandLineToolOptions("dotnet")
            {
                Arguments =
                [
                    "tool",
                    "install",
                    "--tool-path", _temporaryFolder.Path,
                    "dotnet-ilrepack",
                    "--version", "2.*"
                ],
            }, cancellationToken);

            return await command.ExecuteCommandLineTool(options with
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