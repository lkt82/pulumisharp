using System.Diagnostics;
using Pulumi.Automation;
using static SimpleExec.Command;

namespace PulumiSharp;

internal class PulumiCli
{
    [DebuggerStepThrough]
    public async Task RunCommand(string args, LocalWorkspaceOptions workspaceOptions)
    {
        await RunAsync("pulumi", args,
            workspaceOptions.WorkDir?? Directory.GetCurrentDirectory(), true, configureEnvironment:
            c =>
            {
                if (workspaceOptions.EnvironmentVariables == null)
                {
                    return;
                }

                foreach (var keyValue in workspaceOptions.EnvironmentVariables)
                {
                    c.Add(keyValue);
                }
            });
    }

    [DebuggerStepThrough]
    public async Task RunCommand(string args, string? workDir=null, IDictionary<string, string?>? environmentVariable=null)
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