using Automatron;
using Microsoft.Extensions.DependencyInjection;

namespace PulumiSharp.AzureDevOps.Automatron;

public static class PulumiMiddleware
{
    public static IServiceCollection AddPulumi(this IServiceCollection services)
    {
        return services
            .AddSingleton<PulumiCli>();
    }

    public static AutomationRunner UsePulumi(this AutomationRunner automationRunner)
    {
        automationRunner.ConfigureServices(services => AddPulumi(services));

        return automationRunner;
    }
}