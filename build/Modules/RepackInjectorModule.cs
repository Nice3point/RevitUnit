using Build.ILRepack;
using Build.ILRepack.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Modules;
using Shouldly;
using Sourcy.DotNet;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

[DependsOn<CompileProjectModule>]
public sealed class RepackInjectorModule : Module
{
    protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        var targetProject = new File(Projects.Nice3point_TUnit_Revit.FullName);
        var targetFolders = targetProject.Folder!
            .GetFolder("bin")
            .GetFolders(folder => folder.Name.StartsWith("Release"))
            .ToArray();

        targetFolders.ShouldNotBeEmpty("No target folders found to repack");

        foreach (var folder in targetFolders)
        {
            var primaryAssembly = folder.GetFile($"{targetProject.NameWithoutExtension}.dll");
            var primaryPdb = new File(Path.ChangeExtension(primaryAssembly.Path, ".pdb"));

            var dependenciesFolder = folder.GetFolder("Dependencies");
            var dependencyAssembly = dependenciesFolder.GetFile("Nice3point.Revit.Injector.dll");
            dependencyAssembly.Exists.ShouldBeTrue("Dependency assembly not found.");

            var temporaryOutput = context.FileSystem.CreateTemporaryFolder().GetFile($"{targetProject.NameWithoutExtension}.dll");
            var temporaryPdb = new File(Path.ChangeExtension(temporaryOutput.Path, ".pdb"));

            await context.IlRepack().Repack(new IlRepackOptions
            {
                Output = temporaryOutput.Path,
                PrimaryAssembly = primaryAssembly.Path,
                OtherAssemblies = [dependencyAssembly.Path],
                SearchDirectories = [folder.Path, dependenciesFolder.Path],
                CopyAttributes = true,
                XmlDocs = false,
                Union = false,
                Verbose = false
            }, cancellationToken);

            temporaryOutput.Exists.ShouldBeTrue("Repacked assembly not found.");

            primaryAssembly.Delete();
            primaryPdb.Delete();
            temporaryOutput.MoveTo(primaryAssembly.Path);
            temporaryPdb.MoveTo(primaryPdb.Path);
            dependencyAssembly.Delete();
        }

        return await NothingAsync();
    }
}