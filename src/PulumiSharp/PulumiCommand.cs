using System.Diagnostics;
using CommandDotNet;
using Pulumi;
using Spectre.Console;
using CommandContext = CommandDotNet.CommandContext;

namespace PulumiSharp;

internal class PulumiCommand(IAnsiConsole ansiConsole,CommandContext commandContext)
{
    [DefaultCommand]
    [DebuggerStepThrough]
    public virtual Task<int> RunDeployment(params string[]? args)
    {
        var func = commandContext.Services.GetOrThrow<Func<IDictionary<string, object?>>>();

        return Deployment.RunAsync(func);
    }

    [Subcommand] public virtual PulumiStackCommand Stack { get; set; } = null!;

    [Subcommand] public virtual PulumiProfileCommand Profile { get; set; } = null!;

    [Command("login")]
    public async Task Login(string service)
    {
        await PulumiCli.RunCommand($"login {service}");
    }

    [Command("preview", Description = "Show a preview of updates to a stack's resources")]
    [DebuggerStepThrough]
    public async Task<int> Preview(
        [Positional("stack", Description = "The stack to use for the preview. If not specified, the default stack will be used.")]
        string? stack
        )
    {
        var options = new WorkspaceOptionsFactory(commandContext).CreateInline(stack);

        await using var inlineHost = await InlineLanguageHost.Start(options);
        try
        {
            await PulumiCli.RunCommand(
                $"preview --client=127.0.0.1:{inlineHost.Port} --exec-kind auto.inline{(stack != null ? $" --stack {stack}" : string.Empty)}",
                options);
        }
        catch
        {

            if (!inlineHost.TryGetExceptionInfo(out var exception))
            {
                return 1;
            }

            exception.Throw();
        }
        return 0;
    }

    [Command("up",Description = "Create or update the resources in a stack")]
    [DebuggerStepThrough]
    public async Task<int> Up(
        [Positional("stack", Description = "The stack to use for the update. If not specified, the default stack will be used.")]
        string? stack,
        [Option('f', "force", Description = "Force the update without confirmation.")]
        bool force = false)
    {
        var options = new WorkspaceOptionsFactory(commandContext).CreateInline(stack);

        await using var inlineHost = await InlineLanguageHost.Start(options);
        try
        {
            await PulumiCli.RunCommand(
                $"preview --client=127.0.0.1:{inlineHost.Port} --exec-kind auto.inline{(stack != null ? $" --stack {stack}" : string.Empty)}",
                options);

            string? result = null;

            if (!force)
            {
                result = ansiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(
                            "Do you want to perform this update?  [grey][[Use arrows to move, enter to select, type to filter]][/]")
                        .AddChoices("yes", "no"));
            }

            if (force || result == "yes")
            {
                await PulumiCli.RunCommand(
                    $"up -f -y --client=127.0.0.1:{inlineHost.Port} --exec-kind auto.inline{(stack != null ? $" --stack {stack}" : string.Empty)}",
                    options);
            }
        }
        catch
        {

            if (!inlineHost.TryGetExceptionInfo(out var exception))
            {
                return 1;
            }

            exception.Throw();
        }
        return 0;
    }

    [Command("down",Description = "Destroy all existing resources in the stack")]
    [DebuggerStepThrough]
    public async Task Down(
        [Positional("stack", Description = "The stack to use for the destroy. If not specified, the default stack will be used.")]
        string? stack,
        [Option('f', "force", Description = "Force the destroy without confirmation.")]
        bool force = false
        )
    {
        var options = new WorkspaceOptionsFactory(commandContext).CreateInline(stack);

        string? result = null;

        if (!force)
        {
            result = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        "Do you want to perform this destroy?  [grey][[Use arrows to move, enter to select, type to filter]][/]")
                    .AddChoices("yes", "no"));
        }

        if (force || result == "yes")
        {
            await PulumiCli.RunCommand(
                $"down -f -y {(stack != null ? $" --stack {stack}" : string.Empty)}", options);
        }
    }
}

internal class PulumiCommand<T>(IAnsiConsole ansiConsole, CommandContext commandContext)
    : PulumiCommand(ansiConsole, commandContext)
    where T : Pulumi.Stack, new()
{
    [DefaultCommand]
    [DebuggerStepThrough]
    public override Task<int> RunDeployment(params string[]? args) => Deployment.RunAsync<T>();
}