using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using SkuArgs = Pulumi.AzureNative.KeyVault.Inputs.SkuArgs;
using AzureNativeConfig = Pulumi.AzureNative.Config;

namespace PulumiSharp.Azure.Backend;

internal record AzureKeyVaultArgs(Input<string> ResourceGroupName, InputMap<string> Tags);

internal class AzureKeyVault : ComponentResource
{
    public Output<string> Name { get; set; }

    public AzureKeyVault(string name, AzureKeyVaultArgs args, ComponentResourceOptions? options = null) : base(nameof(AzureKeyVault), name, options)
    {
        var vault = new Vault(name, new VaultArgs
        {
            ResourceGroupName = args.ResourceGroupName,
            Properties = new VaultPropertiesArgs
            {
                Sku = new SkuArgs
                {
                    Name = SkuName.Standard,
                    Family = SkuFamily.A
                },
                EnableSoftDelete = true,
                SoftDeleteRetentionInDays = 7,
                TenantId = AzureNativeConfig.TenantId!,
                EnableRbacAuthorization = true
            },
            Tags = args.Tags
        }, new()
        {
            Parent = this,
            Aliases = { new Alias { NoParent = true } }
        });

        Name = vault.Name;

        RegisterOutputs();
    }
}