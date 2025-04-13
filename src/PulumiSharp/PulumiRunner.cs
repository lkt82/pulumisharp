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
    public async Task<int> RunAsync<T>(Func<T> func, params string[] args) where T : class
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(new PulumiCli());
                c.Services.Add(() =>
                {
                    if (typeof(T).IsAssignableTo(typeof(IDictionary<string, object?>)))
                    {
                        return (IDictionary<string, object?>)func();
                    }

                    return func().ToDictionary();
                });
            })
            .RunAsync(args);
    }

    [DebuggerStepThrough]
    public async Task<int> RunAsync(Func<IDictionary<string, object?>> func, params string[] args)
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(new PulumiCli());
                c.Services.Add(func);
            })
            .RunAsync(args);
    }

    [DebuggerStepThrough]
    public async Task<int> RunAsync(Func<object?> func, params string[] args)
    {
        return await new AppRunner<PulumiCommand>()
            .UseSpectreAnsiConsole()
            .UseCancellationHandlers()
            .Configure(c =>
            {
                c.Services.Add(new PulumiCli());
                c.Services.Add(() =>
                {
                    var result = func();
                    if (result == null)
                    {
                        return new Dictionary<string, object?>();
                    }

                    if (result.GetType().IsAssignableTo(typeof(IDictionary<string, object?>)))
                    {
                        return (IDictionary<string, object?>)result;
                    }

                    return result.ToDictionary();
                });
            })
            .RunAsync(args);
    }
}

public class PulumiRunner<T> where T : Stack
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
                IDictionary<string, object?> Func()
                {
                    var stack = _serviceProvider == null
                        ? Activator.CreateInstance<T>()
                        : _serviceProvider.GetRequiredService<T>();
                    return stack.DoBuild();
                }

                c.Services.Add(new PulumiCli());
                c.Services.Add(Func);
            })
            .RunAsync(args);
    }
}