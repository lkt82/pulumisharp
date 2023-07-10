using Azure.Identity;
using Pulumi;
using Pulumi.AzureNative.Authorization;
using AzureNativeConfig = Pulumi.AzureNative.Config;

namespace PulumiSharp.Azure;

internal class AzureNativeCliCredential : AzureCliCredential
{
    public AzureNativeCliCredential(ResourceOptions? options = null) :base(new AzureCliCredentialOptions
    {
        TenantId = options == null ? AzureNativeConfig.TenantId : GetClientConfig.InvokeAsync(new InvokeOptions
        {
            Provider = options.Provider,
            Parent = options.Parent
        }).Result.TenantId
    })
    {
    }
}