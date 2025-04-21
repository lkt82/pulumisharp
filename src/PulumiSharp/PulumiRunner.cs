using System.Collections.Immutable;
using System.Diagnostics;
using CommandDotNet;
using CommandDotNet.Spectre;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace PulumiSharp;

[UsedImplicitly]
public class PulumiRunner
{
    [DebuggerStepThrough]
    public async Task<int> RunAsync(Action func, params string[] args)
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(() => ImmutableDictionary<string, object?>.Empty);
            })
            .RunAsync(args);
    }

    [DebuggerStepThrough]
    public async Task<int> RunAsync<T>(Func<T> func, params string[] args) where T : class
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(() => OutputSerializer.Serialize(func()));
            })
            .RunAsync(args);
    }
}

public class PulumiRunner<TStack> where TStack : Stack
{
    private readonly ServiceProvider? _serviceProvider;

    public PulumiRunner(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PulumiRunner()
    {
    }

    public async Task<int> RunAsync(params string[] args)
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(Func);
                return;

                IDictionary<string, object?> Func()
                {
                    var stack = _serviceProvider == null
                        ? Activator.CreateInstance<TStack>()
                        : _serviceProvider.GetRequiredService<TStack>();
                    return OutputSerializer.Serialize(stack.DoBuild());
                }
            })
            .RunAsync(args);
    }
}