using CommandDotNet;
using Pulumi.Automation;
using Pulumi.Automation.Commands.Exceptions;
using Spectre.Console;
using CommandContext = CommandDotNet.CommandContext;

namespace PulumiSharp;

[Subcommand]
[Command("stack")]
internal class PulumiStackCommand : PulumiCommandBase
{
    public PulumiStackCommand(IAnsiConsole ansiConsole, CommandContext commandContext) : base(ansiConsole, commandContext)
    {
    }

    [Command("init")]
    public async Task Init(string stack)
    {
        var workspaceOptions = CreateInlineWorkspaceOptions(stack);

        var projectConfig = Path.Combine(workspaceOptions.WorkDir!, "Pulumi.yaml");

        var config = new Dictionary<string, ConfigValue>
        {
            { "azure-native:location", new ConfigValue("westeurope") }
        };


        if (!File.Exists(projectConfig))
        {
            workspaceOptions.StackSettings = null;

            using var workspaceStack = await LocalWorkspace.CreateOrSelectStackAsync(workspaceOptions);
            await workspaceStack.SetAllConfigAsync(config);
        }
        else
        {
            workspaceOptions.StackSettings = null;
            workspaceOptions.ProjectSettings = null;

            using var workspace = await LocalWorkspace.CreateAsync(workspaceOptions);

            try
            {
                await workspace.CreateStackAsync(workspaceOptions.StackName);
            }
            catch (StackAlreadyExistsException)
            {
                await workspace.SelectStackAsync(workspaceOptions.StackName);
            }
        }
    }

    [Command("select")]
    public async Task Select(string stack)
    {
        var workspaceOptions = CreateInlineWorkspaceOptions(stack);

        await PulumiCli.RunCommand($"stack select --stack {stack}", workspaceOptions);
    }
}