using System.Diagnostics;
using Pulumi.Automation;
using static SimpleExec.Command;

namespace PulumiSharp;

internal static class PulumiCli
{
    [DebuggerStepThrough]
    public static async Task RunCommand(string args, LocalWorkspaceOptions options)
    {
        await RunAsync("pulumi", args,
            options.WorkDir?? Directory.GetCurrentDirectory(), true, configureEnvironment:
            c =>
            {
                if (options.EnvironmentVariables == null)
                {
                    return;
                }

                foreach (var keyValue in options.EnvironmentVariables)
                {
                    c.Add(keyValue);
                }
            });
    }

    [DebuggerStepThrough]
    public static async Task RunCommand(string args, string? workDir=null, IDictionary<string, string?>? environmentVariable=null)
    {
        await RunAsync("pulumi", args,
            workDir?? Directory.GetCurrentDirectory(), true, configureEnvironment:
            c =>
            {
                if (environmentVariable == null)
                {
                    return;
                }

                foreach (var keyValue in environmentVariable)
                {
                    c.Add(keyValue);
                }
            });
    }

}