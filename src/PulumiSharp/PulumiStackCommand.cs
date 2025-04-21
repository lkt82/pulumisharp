using CommandDotNet;
using Pulumi.Automation;
using Pulumi.Automation.Commands.Exceptions;
using CommandContext = CommandDotNet.CommandContext;

namespace PulumiSharp;

[Subcommand]
[Command("stack")]
internal class PulumiStackCommand(CommandContext commandContext)
{
    [Command("init")]
    public async Task Init(string stack)
    {
        var options = new WorkspaceOptionsFactory(commandContext).CreateInline(stack);

        var projectConfig = Path.Combine(options.WorkDir!, "Pulumi.yaml");

        var config = new Dictionary<string, ConfigValue>
        {
            { "azure-native:location", new ConfigValue("westeurope") }
        };


        if (!File.Exists(projectConfig))
        {
            options.StackSettings = null;

            using var workspaceStack = await LocalWorkspace.CreateOrSelectStackAsync(options);
            await workspaceStack.SetAllConfigAsync(config);
        }
        else
        {
            options.StackSettings = null;
            options.ProjectSettings = null;

            using var workspace = await LocalWorkspace.CreateAsync(options);

            try
            {
                await workspace.CreateStackAsync(options.StackName);
            }
            catch (StackAlreadyExistsException)
            {
                await workspace.SelectStackAsync(options.StackName);
            }
        }
    }

    [Command("select")]
    public async Task Select(string stack)
    {
        var options = new WorkspaceOptionsFactory(commandContext).CreateInline(stack);

        await PulumiCli.RunCommand($"stack select --stack {stack}", options);
    }
}