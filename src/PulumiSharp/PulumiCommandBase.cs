using System.Reflection;
using CommandDotNet;
using Pulumi.Automation;
using Spectre.Console;

namespace PulumiSharp;

internal abstract class PulumiCommandBase
{
    protected readonly IAnsiConsole AnsiConsole;
    protected readonly CommandContext CommandContext;
    protected readonly PulumiCli PulumiCli;

    protected PulumiCommandBase(IAnsiConsole ansiConsole, CommandContext commandContext)
    {
        AnsiConsole = ansiConsole;
        CommandContext = commandContext;
        PulumiCli = commandContext.Services.GetOrCreate<PulumiCli>();
    }

    protected static string ProjectName => Assembly.GetEntryAssembly()!.GetName().Name!;

    protected virtual InlineProgramArgs CreateInlineWorkspaceOptions(string? stack)
    {
        var func = CommandContext.Services.GetOrThrow<Func<IDictionary<string, object?>>>();

        var program = PulumiFn.Create(func);

        return WorkspaceOptionsFactory.CreateLocalInline(ProjectName, stack, program);
    }
}