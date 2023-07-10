using Pulumi;

namespace PulumiSharp;

public static class DeploymentInstanceExtensions
{
    public static string GetOrganizationName(this DeploymentInstance instance)
    {
        if (!string.IsNullOrEmpty(instance.OrganizationName))
        {
            return instance.OrganizationName;
        }

        var profile = PulumiContext.GetProfile();

        var organization = profile?.Organization ?? Environment.GetEnvironmentVariable("PULUMI_ORGANIZATION");

        if (organization == null)
        {
            throw new InvalidOperationException();
        }

        return organization;
    }
}