using System.Text;
using Pulumi;
using Pulumi.AzureNative.ContainerService;
using Pulumi.Kubernetes;
using KubernetesProvider = Pulumi.Kubernetes.Provider;

namespace PulumiSharp.Azure.Aks;

public record AksProviderArgs(
    Output<string>? ResourceGroupName = null,
    Output<string>? ClusterName = null,
    Output<string>? KubeConfig = null
)
{
    public Output<string>? KubeConfig { get; set; } = KubeConfig ?? AksProvider.GetKubeConfig(
        ResourceGroupName ?? throw new InvalidOperationException(),
        ClusterName ?? throw new InvalidOperationException());
}


public class AksProvider : KubernetesProvider
{
    public static Output<string> GetKubeConfig(Input<string> resourceGroupName, Input<string> clusterName)
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

    public AksProvider(string name, AksProviderArgs args, CustomResourceOptions? options = null) : base(name, new ProviderArgs
    {
        KubeConfig = args.KubeConfig ?? throw new InvalidOperationException(),
        SuppressDeprecationWarnings = false,
        SuppressHelmHookWarnings = true,
        EnableServerSideApply = false,
        DeleteUnreachable = true
    }, options)
    {
        KubeConfig = args.KubeConfig;
    }

    public Output<string> KubeConfig { get; set; }
}