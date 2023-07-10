using System.Reflection;
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.IO;
using Automatron.AzureDevOps.Models;
using CommandDotNet;
using Pulumi;
using Pulumi.Automation;
using Pulumi.AzureDevOps;
using Pulumi.AzureDevOps.Inputs;
using Spectre.Console;
using Config = Pulumi.Config;

namespace PulumiSharp.AzureDevOps.Automatron;

public class PulumiAzureDevOpsCommand : AzureDevOpsCommand
{
    private readonly PulumiFn _program;

    // ReSharper disable once PossibleMultipleEnumeration
    public PulumiAzureDevOpsCommand(IAnsiConsole console, IEnumerable<Pipeline> pipelines, IPipelineEngine pipelineEngine) : base(console, pipelines, pipelineEngine)
    {
        _program = PulumiFn.Create(() =>
        {
            var config = new Config("azuredevopspipeline");
            var organizationName = config.Require("organization");
            var projectName = config.Require("project");
            var repositoryName = config.Require("repository");
            var stackSuffix = config.RequireBoolean("stacksuffix");

            var stackName = Deployment.Instance.StackName;

            var path = repositoryName;

            var azureDevOpsProvider = new AzureDevOpsProvider(organizationName, new(organizationName));

            var project = GetProject.Invoke(new GetProjectInvokeArgs
            {
                Name = projectName
            }, new InvokeOptions
            {
                Provider = azureDevOpsProvider
            });

            var repository = GetGitRepository.Invoke(new GetGitRepositoryInvokeArgs
            {
                Name = repositoryName,
                ProjectId = project.Apply(c => c.Id)
            }, new InvokeOptions
            {
                Provider = azureDevOpsProvider
            });

            foreach (var pipeline in pipelines)
            {
                var name = string.IsNullOrEmpty(pipeline.DisplayName) ? pipeline.Name : pipeline.DisplayName;

                if (stackSuffix)
                {
                    name += " " + stackName;
                }

                var buildDefinition = new BuildDefinition($"{repositoryName}-{pipeline.Name}".Replace(" ", "-").ToLower(), new BuildDefinitionArgs
                {
                    ProjectId = project.Apply(c => c.Id),
                    Name = name,
                    Path = $"\\{path}",
                    CiTrigger = new BuildDefinitionCiTriggerArgs
                    {
                        UseYaml = true
                    },
                    Repository = new BuildDefinitionRepositoryArgs
                    {
                        RepoType = "TfsGit",
                        RepoId = repository.Apply(r => r.Id),
                        YmlPath = GetYmlPath(pipeline)
                    }

                }, new CustomResourceOptions
                {
                    Provider = azureDevOpsProvider,
                    DeleteBeforeReplace = true
                });
            }

            return new Dictionary<string, object?>();
        });
    }

    private static string GetYmlPath(Pipeline pipeline)
    {
        var dir = Path.GetFullPath(pipeline.YmlDir!);

        var root = Path.GetFullPath(pipeline.RootDir?? PathExtensions.GetGitRoot(dir)!);

        var path = PathExtensions.GetUnixPath(Path.Combine(Path.GetRelativePath(root, dir), pipeline.YmlName!));

        return path;
    }

    private async Task<WorkspaceStack> CreateWorkspaceStack(string stackName)
    {
        var projectName = Assembly.GetEntryAssembly()!.GetName().Name!;

        var stackArgs = WorkspaceOptionsFactory.CreateInline(projectName, stackName, _program);

        var stackConfig = $"Pulumi.{stackName}.yaml";

        var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

        var config = await File.ReadAllTextAsync(stackConfig);

        await File.AppendAllTextAsync(Path.Combine(stack.Workspace.WorkDir, stackConfig), config);

        return stack;
    }

    [Command(Description = "Create or update Azure DevOps pipelines")]
    public async Task<int> Up(string stackName)
    {
        using var stack = await CreateWorkspaceStack(stackName);

        var result = await stack.UpAsync(new UpOptions
        {
            OnStandardOutput = Console.WriteLine,
            OnStandardError = Console.Error.WriteLine
        });

        return 0;
    }

    [Command(Description = "Destroy all existing Azure DevOps pipelines")]
    public async Task<int> Down(string stackName)
    {
        using var stack = await CreateWorkspaceStack(stackName);

        var result = await stack.DestroyAsync(new DestroyOptions
        {
            OnStandardOutput = Console.WriteLine,
            OnStandardError = Console.Error.WriteLine
        });

        return 0;
    }
}