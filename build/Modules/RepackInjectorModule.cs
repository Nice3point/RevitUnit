using Build.ILRepack;
using Build.ILRepack.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Modules;
using Shouldly;
using Sourcy.DotNet;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

[DependsOn<CompileProjectsModule>]
public sealed class RepackInjectorModule : Module
{
    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var targetProject = new File(Projects.Nice3point_TUnit_Revit.FullName);
        var targetFolders = targetProject.Folder!
            .GetFolder("bin")
            .GetFolders(folder => folder.Name.StartsWith("Release"))
            .ToArray();

        targetFolders.ShouldNotBeEmpty("No target folders found to repack");

        foreach (var folder in targetFolders)
        {
            var primaryAssembly = folder.FindFile(file => file.NameWithoutExtension == targetProject.NameWithoutExtension && file.Extension == ".dll");
            targetFolders.ShouldNotBeNull("No primary assembly found to repack");

            var primaryPdb = new File(Path.ChangeExtension(primaryAssembly!.Path, ".pdb"));
            var dependenciesFolder = primaryAssembly.Folder!.GetFolder("Dependencies");
            var dependencyAssembly = dependenciesFolder.GetFile("Nice3point.Revit.Injector.dll");
            dependencyAssembly.Exists.ShouldBeTrue("Dependency assembly not found.");

            var temporaryOutput = Folder.CreateTemporaryFolder().GetFile($"{targetProject.NameWithoutExtension}.dll");
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

            await primaryAssembly.DeleteAsync(cancellationToken);
            await primaryPdb.DeleteAsync(cancellationToken);
            await temporaryOutput.MoveToAsync(primaryAssembly.Path, cancellationToken);
            await temporaryPdb.MoveToAsync(primaryPdb.Path, cancellationToken);
            await dependencyAssembly.DeleteAsync(cancellationToken);
        }
    }
}