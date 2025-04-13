using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using JetBrains.Annotations;
using static SimpleExec.Command;

namespace Pipelines;

[Pipeline("Ci", YmlDir = RelativeRootDir, YmlName = "azure-pipelines")]
[CiTrigger(Batch = true, IncludeBranches = ["main"], IncludePaths = ["src"])]
[PrTrigger(Disabled = true)]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job(DisplayName = "Ci")]
[UsedImplicitly]
public class Pipeline(LoggingCommands loggingCommands)
{
    private const string RelativeRootDir = "../../";

    private static string RootDir => Path.GetFullPath(RelativeRootDir, Directory.GetCurrentDirectory());

    private const string Configuration = "Release";

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    private readonly string[] _projects = {
        $"{RootDir}/src/PulumiSharp",
        $"{RootDir}/src/PulumiSharp.Azure",
        $"{RootDir}/src/PulumiSharp.Azure.Aks",
        $"{RootDir}/src/PulumiSharp.Azure.Backend"
    };

    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    private static void CleanDirectory(string dir)
    {
        var path = Path.GetFullPath(dir);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void EnsureDirectory(string dir)
    {
        var path = Path.GetFullPath(dir);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    [Checkout(CheckoutSource.Self, FetchDepth = 0)]
    [Step(Emoji = "🔢")]
    public async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await loggingCommands.UpdateBuildNumberAsync(version);
    }

    [Step(Emoji = "🧹")]
    public void Clean()
    {
        CleanDirectory(ArtifactsDir);
        EnsureDirectory(ArtifactsDir);
    }

    [Step(Emoji = "🏗", DependsOn = [nameof(Version), nameof(Clean)])]
    public async Task Build()
    {
        foreach (var project in _projects)
        {
            await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: project, noEcho: true);
        }
    }

    [Step(Emoji = "📦", DependsOn = [nameof(Build), nameof(Clean)])]
    public async Task Pack()
    {
        foreach (var project in _projects)
        {
            await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: project, noEcho: true);
        }
    }

    [Step(Emoji = "🚀", DependsOn = [nameof(Pack)])]
    public async Task Publish()
    {
        if (NugetApiKey == null)
        {
            throw new ArgumentNullException(nameof(NugetApiKey), "NugetApiKey missing");
        }

        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await loggingCommands.UploadArtifactAsync("/", "Nuget", nuget);
            await loggingCommands.UploadArtifactAsync("/", "Nuget", nuget.Replace("nupkg", "snupkg"));
            await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey?.GetValue()} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: RootDir, noEcho: true);
        }
    }
}