using System.Text;
using Pulumi;
using Pulumi.AzureNative.ContainerService;
using Pulumi.Kubernetes;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace PulumiSharp.Azure.Aks;

public record AksProviderArgs(
    Output<string> ResourceGroupName,
    Output<string> ClusterName
);

public class AksProvider : KubernetesProvider
{
    private static Output<string> GetKubeConfig(Input<string> resourceGroupName, Input<string> clusterName)
    {
        return Output.CreateSecret(ListManagedClusterUserCredentials.Invoke(new()
        {
            ResourceGroupName = resourceGroupName,
            ResourceName = clusterName
        }).Apply(credentials =>
        {
            var encoded = credentials.Kubeconfigs[0].Value;
            var data = Convert.FromBase64String(encoded);
            var raw = Encoding.UTF8.GetString(data);

            if (Pulumi.AzureNative.Config.ClientId != null)
            {
                return raw.Replace("devicecode", $"spn --client-id {Pulumi.AzureNative.Config.ClientId} --client-secret {Pulumi.AzureNative.Config.ClientSecret}");
            }

            if (Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") != null)
            {
                return raw.Replace("devicecode", "spn");
            }

            return raw.Replace("devicecode", "azurecli");
        }));
    }

    public AksProvider(string name, AksProviderArgs args,CustomResourceOptions? options = null) : base(name, new ProviderArgs
    {
        KubeConfig = GetKubeConfig(args.ResourceGroupName, args.ClusterName),
        SuppressDeprecationWarnings = false,
        SuppressHelmHookWarnings = true,
        EnableServerSideApply = false,
        DeleteUnreachable = true
    }, options)
    {
    }
}