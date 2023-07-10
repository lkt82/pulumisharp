using Pulumi;
using Pulumi.AzureDevOps;
using Environment = System.Environment;

namespace PulumiSharp.AzureDevOps
{
    public record AzureDevOpsProviderArgs(Input<string> Organization);

    public class AzureDevOpsProvider : Provider
    {
        public AzureDevOpsProvider(string name, AzureDevOpsProviderArgs args) : base(name,new ProviderArgs
        {
            OrgServiceUrl = Output.Format($"https://dev.azure.com/{args.Organization}"),
            PersonalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_EXT_PAT") ?? AzureDevOpsCredentials.Current?.AccessToken ?? throw new InvalidOperationException()
        })
        {
        }
    }
}
