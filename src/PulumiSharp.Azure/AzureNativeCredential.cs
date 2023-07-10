using Azure.Identity;
using Pulumi;

namespace PulumiSharp.Azure;

public class AzureNativeCredential : ChainedTokenCredential
{
    public AzureNativeCredential(ResourceOptions? options = null) : base(new EnvironmentCredential(), new AzureNativeCliCredential(options))
    {
    }

    public static AzureNativeCredential Current { get; } = new Lazy<AzureNativeCredential>(() => new AzureNativeCredential()).Value;
}