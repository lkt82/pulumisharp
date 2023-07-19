using System.Diagnostics;
using CommandDotNet;
using CommandDotNet.Spectre;
using Pulumi.Automation;

namespace PulumiSharp;

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
                c.Services.Add(()=>
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
}

public class PulumiRunner<T> where T : Pulumi.Stack, new()
{
    public async Task<int> RunAsync(params string[] args)
    {
        return await new AppRunner<PulumiCommand<T>>()
            .UseSpectreAnsiConsole()
            .Configure(c=>
            {
                c.Services.Add(new PulumiCli());
                c.Services.Add(PulumiFn.Create<T>());
            })
            .RunAsync(args);
    }
}