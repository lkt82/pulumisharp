using System.Reflection;
using CommandDotNet;
using Pulumi.Automation;
using Spectre.Console;

namespace PulumiSharp;

internal abstract class PulumiCommandBase(IAnsiConsole ansiConsole, CommandContext commandContext)
{
    protected readonly IAnsiConsole AnsiConsole = ansiConsole;
    protected readonly CommandContext CommandContext = commandContext;
    protected readonly PulumiCli PulumiCli = commandContext.Services.GetOrCreate<PulumiCli>();

    protected static string ProjectName => Assembly.GetEntryAssembly()!.GetName().Name!;

    protected virtual InlineProgramArgs CreateInlineWorkspaceOptions(string? stack)
    {
        var func = CommandContext.Services.GetOrThrow<Func<IDictionary<string, object?>>>();

        var program = PulumiFn.Create(func);

        return WorkspaceOptionsFactory.CreateLocalInline(ProjectName, stack, program);
    }
}