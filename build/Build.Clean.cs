using Nuke.Common.ProjectModel;

sealed partial class Build
{
    Target Clean => _ => _
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            Project[] excludedProjects =
            [
                Solution.Build
            ];

            CleanDirectory(ArtifactsDirectory);
            foreach (var project in Solution.AllProjects)
            {
                if (excludedProjects.Contains(project)) continue;

                CleanDirectory(project.Directory / "bin");
                CleanDirectory(project.Directory / "obj");
            }
        });

    static void CleanDirectory(AbsolutePath path)
    {
        Log.Information("Cleaning directory: {Directory}", path);
        path.CreateOrCleanDirectory();
    }
}