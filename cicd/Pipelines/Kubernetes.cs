using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using JetBrains.Annotations;
using Semver;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static SimpleExec.Command;

namespace Pipelines;

public class KubernetesProjectDocument
{
    public List<KubernetesProject> Projects = new();
}

public class KubernetesProject
{
    public string Name { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public List<string> Url { get; set; } = new();

    public IEnumerable<string> GetVersionedUrl()
    {
        var semVersion = SemVersion.Parse(Version, SemVersionStyles.Strict);

        return Url.Select(c => c.Replace("${VERSION}", semVersion.ToString())
            .Replace("${MAJOR}", semVersion.Major.ToString())
            .Replace("${MINOR}", semVersion.Minor.ToString()));
    }

    public string Version { get; set; } = null!;
}

[Pipeline(DisplayName = "PulumiSharp Kubernetes Sdk")]
[CiTrigger(Disabled = true)]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job(DisplayName = "Ci")]
[UsedImplicitly]
public class Kubernetes
{
    private readonly LoggingCommands _loggingCommands;

    private const string RootPath = "../../";

    private static string RootDir => Path.GetFullPath(RootPath, Directory.GetCurrentDirectory());

    private const string Configuration = "Release";

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    private static string? AssemblyInformationalVersion => Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    private static string? AssemblyFileVersion => Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
    private static string? AssemblyVersion => Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;

    private static KubernetesProjectDocument CrdConfig => new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance).Build()
        .Deserialize<KubernetesProjectDocument>(File.ReadAllText("Projects.yaml"));

    internal class DefaultRemover : IYamlVisitor
    {
        public void Visit(YamlStream stream)
        {
            foreach (var doc in stream)
            {
                doc.Accept(this);
            }
        }

        public void Visit(YamlDocument document)
        {
            document.RootNode.Accept(this);
        }

        public void Visit(YamlScalarNode scalar)
        {
        }

        public void Visit(YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(YamlMappingNode mapping)
        {
            foreach (var child in mapping.Children.ToList())
            {
                if (child.Key.ToString() == "default" && (child.Value.NodeType == YamlNodeType.Mapping || child.Value.NodeType == YamlNodeType.Sequence))
                {
                    mapping.Children.Remove(child);
                }
                else
                {
                    child.Key.Accept(this);
                    child.Value.Accept(this);
                }
            }
        }
    }


    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    public Kubernetes(LoggingCommands loggingCommands)
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

    private static async Task ParseSourceCode(string projectDir, KubernetesProject crd)
    {
        foreach (var csFile in Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories))
        {
            var content = "using Pulumi;\n";

            content += (await File.ReadAllTextAsync(csFile))
                .Replace("Pulumi.Kubernetes.Types.Outputs.Meta", "#OutputsMETA")
                .Replace("Pulumi.Kubernetes.Types.Inputs.Meta", "#InputsMETA")
                .Replace($"Pulumi.{crd.Name}", crd.ProjectName)
                .Replace($"Pulumi.Kubernetes.Types", $"{crd.ProjectName}")
                .Replace("#OutputsMETA", "Pulumi.Kubernetes.Types.Outputs.Meta")
                .Replace("#InputsMETA", "Pulumi.Kubernetes.Types.Inputs.Meta");

            var regex = new Regex(@"(?<!""\w[^""]*)-(\w)");

            content = regex.Replace(content, (Match match) =>
            {
                var letter = match.Groups[1].Value;
                var upper = CultureInfo.InvariantCulture.TextInfo.ToUpper(letter);
                return upper;
            });

            await File.WriteAllTextAsync(csFile, content);
        }
    }

    private static void ParseProject(string projectDir, KubernetesProject crd, string projectFilePath)
    {
        File.Move(Path.Combine(projectDir, $"Pulumi.{crd.Name}.csproj"), projectFilePath);

        var doc = new XmlDocument
        {
            PreserveWhitespace = true
        };

        doc.Load(projectFilePath);

        var propertyGroup = doc.DocumentElement!.SelectSingleNode("PropertyGroup")!;

        propertyGroup.SelectSingleNode("GeneratePackageOnBuild")!.InnerText = "false";
        propertyGroup.SelectSingleNode("TargetFramework")!.InnerText = "net6.0";
        propertyGroup.RemoveChild(propertyGroup.SelectSingleNode("Company")!);
        propertyGroup.SelectSingleNode("Authors")!.InnerText = "lkt82";

        var version = doc.CreateElement("Version");
        version.InnerText = AssemblyInformationalVersion!;
        propertyGroup.AppendChild(version);

        var assemblyVersion = doc.CreateElement("AssemblyVersion");
        assemblyVersion.InnerText = AssemblyVersion!;
        propertyGroup.AppendChild(assemblyVersion);

        var fileVersion = doc.CreateElement("FileVersion");
        fileVersion.InnerText = AssemblyFileVersion!;
        propertyGroup.AppendChild(fileVersion);

        var nodesToRemove = new[]
        {
            doc.DocumentElement!.SelectSingleNode(
                "ItemGroup/PackageReference[@Include='Microsoft.SourceLink.GitHub']")!.ParentNode!,
            doc.DocumentElement!.SelectSingleNode(
                "PropertyGroup[contains(@Condition,'GITHUB_ACTIONS')]")!,
            doc.DocumentElement!.SelectSingleNode(
            "PropertyGroup/AllowedOutputExtensionsInPackageBuildOutputFolder")!,
            doc.DocumentElement!.SelectSingleNode("ItemGroup/PackageReference[@Include='Pulumi']")!
        };

        foreach (var node in nodesToRemove)
        {
            node.ParentNode!.RemoveChild(node);
        }

        var itemGroup = doc.DocumentElement!.SelectSingleNode("ItemGroup/PackageReference[@Include='Pulumi.Kubernetes']")!
            .ParentNode!;

        var packageReferenceInclude = doc.CreateAttribute("Include");
        packageReferenceInclude.Value = "Microsoft.SourceLink.GitHub";
        var packageReferenceVersion = doc.CreateAttribute("Version");
        packageReferenceVersion.Value = "1.1.1";

        var packageReference = doc.CreateElement("PackageReference");
        packageReference.Attributes.Append(packageReferenceInclude);
        packageReference.Attributes.Append(packageReferenceVersion);

        itemGroup.AppendChild(packageReference);

        var import = doc.CreateElement("Import");
        var importProject = doc.CreateAttribute("Project");
        importProject.Value = @"..\..\build.props";
        import.Attributes.Append(importProject);

        doc.DocumentElement.AppendChild(import);

        doc.Save(projectFilePath);
    }

    [Checkout(CheckoutSource.Self, FetchDepth = 0)]
    [Step(Emoji = "🔢")]
    public async Task Version()
    {
        if (AssemblyInformationalVersion == null)
        {
            return;
        }

        await _loggingCommands.UpdateBuildNumberAsync(AssemblyInformationalVersion);
    }

    [Step(Emoji = "🧹")]
    public void Clean()
    {
        CleanDirectory(ArtifactsDir);
        EnsureDirectory(ArtifactsDir);
    }


    [Step(Emoji = "📥", DependsOn = new[] { nameof(Clean) })]
    public async Task Download()
    {
        using var httpClient = new HttpClient();

        foreach (var project in CrdConfig.Projects)
        {
            var yaml = new YamlStream();

            var path = Path.Combine(ArtifactsDir, project.Name + ".yaml");

            var memoryStream = new MemoryStream();

            foreach (var file in project.GetVersionedUrl())
            {
                await using var stream = await httpClient.GetStreamAsync(file);

                await stream.CopyToAsync(memoryStream);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(memoryStream);

            yaml.Load(streamReader);

            streamReader.Close();

            var visitor = new DefaultRemover();

            yaml.Accept(visitor);

            await using var streamWriter = File.CreateText(path);
            yaml.Save(streamWriter,false);

            streamWriter.Close();
        }
    }

    [Step(Emoji = "🏗", DependsOn = new[] { nameof(Download) })]
    public async Task Generate()
    {
        foreach (var crd in CrdConfig.Projects)
        {
            var projectDir = Path.Combine(ArtifactsDir, crd.Name);

            var yamlPath = Path.Combine(ArtifactsDir, crd.Name + ".yaml");

            var projectFilePath = Path.Combine(projectDir, $"{crd.ProjectName}.csproj");

            await RunAsync($"{RootPath}/.tools/crd2pulumi", $"--dotnetName {crd.Name} --dotnetPath {crd.Name} {yamlPath} --force", workingDirectory: ArtifactsDir, noEcho: true);

            await File.WriteAllTextAsync(Path.Combine(projectDir, "version.txt"), crd.Version);

            await ParseSourceCode(projectDir, crd);

            ParseProject(projectDir, crd, projectFilePath);
        }
    }

    [Step(Emoji = "🏗", DependsOn = new[] { nameof(Generate) })]
    public async Task Build()
    {
        foreach (var projectDir in CrdConfig.Projects.Select(crd => Path.Combine(ArtifactsDir, crd.Name)))
        {
            await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: projectDir, noEcho: true);
        }
    }

    [Step(Emoji = "📦", DependsOn = new[] { nameof(Build), nameof(Clean) })]
    public async Task Pack()
    {
        foreach (var projectDir in CrdConfig.Projects.Select(crd => Path.Combine(ArtifactsDir, crd.Name)))
        {
            await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: projectDir, noEcho: true);
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

            await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey.GetValue()} -s inpay --skip-duplicate", workingDirectory: RootDir, noEcho: true);
        }
    }
}