using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using JetBrains.Annotations;
using static SimpleExec.Command;

namespace Pipelines;

[Pipeline(DisplayName = "PulumiSharp Sdk")]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job(DisplayName = "Ci")]
[UsedImplicitly]
public class Sdk
{
    private readonly LoggingCommands _loggingCommands;

    private const string RelativeRootDir = "../../";

    private static string RootDir => Path.GetFullPath(RelativeRootDir, Directory.GetCurrentDirectory());

    private const string Configuration = "Release";

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    private readonly string[] _projects = {
        $"{RootDir}/src/PulumiSharp",
        $"{RootDir}/src/PulumiSharp.Azure",
        $"{RootDir}/src/PulumiSharp.Azure.Aks",
        $"{RootDir}/src/PulumiSharp.Azure.Backend",
        $"{RootDir}/src/PulumiSharp.AzureDevOps",
        $"{RootDir}/src/PulumiSharp.AzureDevOps.Automatron"
    };

    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    public Sdk(LoggingCommands loggingCommands)
    {
        _loggingCommands = loggingCommands;
    }

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
    [NuGetAuthenticate(NugetServiceConnections = "myget")]
    [Step(Emoji = "🔢")]
    public async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await _loggingCommands.UpdateBuildNumberAsync(version);
    }

    [Step(Emoji = "🧹")]
    public void Clean()
    {
        CleanDirectory(ArtifactsDir);
        EnsureDirectory(ArtifactsDir);
    }

    [Step(Emoji = "🏗", DependsOn = new[] { nameof(Version), nameof(Clean) })]
    public async Task Build()
    {
        foreach (var project in _projects)
        {
            await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: project, noEcho: true);
        }
    }

    [Step(Emoji = "📦", DependsOn = new[] { nameof(Build), nameof(Clean) })]
    public async Task Pack()
    {
        foreach (var project in _projects)
        {
            await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: project, noEcho: true);
        }
    }

    [Step(Emoji = "🚀", DependsOn = new[] { nameof(Pack) })]
    public async Task Publish()
    {
        if (NugetApiKey == null)
        {
            throw new ArgumentNullException(nameof(NugetApiKey), "NugetApiKey missing");
        }

        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await _loggingCommands.UploadArtifactAsync("/", "Nuget", nuget);
            await _loggingCommands.UploadArtifactAsync("/", "Nuget", nuget.Replace("nupkg", "snupkg"));
        }

        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey.GetValue()} -s inpay --skip-duplicate", workingDirectory: RootDir, noEcho: true);
        }
    }
}