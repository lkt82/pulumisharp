using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.Random;
using PulumiAzureNativeStorage = Pulumi.AzureNative.Storage;
using AzureNativeConfig = Pulumi.AzureNative.Config;

namespace PulumiSharp.Azure.Backend;

public record AzurePulumiBackendArgs(Input<string> ResourceGroupName, InputMap<string> Tags);

public class AzurePulumiBackendConfig
{
    public List<string> Organizations { get; set; } = new();
}

public class AzurePulumiBackend : Component<AzurePulumiBackend,AzurePulumiBackendArgs, AzurePulumiBackendConfig>
{
    public Output<string> StorageAccountName { get; set; }

    public Output<string> KeyVaultName { get; set; }

    public AzurePulumiBackend(string name, AzurePulumiBackendArgs args, ComponentResourceOptions? options=null) : base(name,nameof(AzurePulumiBackend).ToLower(), args, options)
    {
        var id = new RandomId($"{name}-storageaccount", new RandomIdArgs
        {
            ByteLength = 4,
            Prefix = name
        }, new()
        {
            Parent = this,
            Aliases = { new Alias { NoParent = true } }
        });

        var storageAccount = new PulumiAzureNativeStorage.StorageAccount($"{name}-storageaccount", new PulumiAzureNativeStorage.StorageAccountArgs
        {
            AccountName = id.Hex.Apply(c => c.ToLower()),
            ResourceGroupName = args.ResourceGroupName,
            Tags = args.Tags,
            Kind = PulumiAzureNativeStorage.Kind.StorageV2,
            AllowSharedKeyAccess = false,
            AllowBlobPublicAccess = false,
            IsHnsEnabled = true,
            MinimumTlsVersion = PulumiAzureNativeStorage.MinimumTlsVersion.TLS1_2,
            DefaultToOAuthAuthentication = true,
            Sku = new PulumiAzureNativeStorage.Inputs.SkuArgs
            {
                Name = PulumiAzureNativeStorage.SkuName.Standard_ZRS
            },
        }, new()
        {
            ReplaceOnChanges = { "immutableStorageWithVersioning" },
            DeleteBeforeReplace = true,
            Parent = this,
            Aliases = { new Alias { NoParent = true } }
        });

        var vault = new Vault($"{name}", new VaultArgs
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
            Parent = this
        });

        foreach (var organization in Config.Organizations)
        {
            var blobContainer = new PulumiAzureNativeStorage.BlobContainer(organization, new PulumiAzureNativeStorage.BlobContainerArgs
            {
                AccountName = storageAccount.Name,
                ResourceGroupName = args.ResourceGroupName,
                PublicAccess = PulumiAzureNativeStorage.PublicAccess.None,
                ContainerName = organization
            }, new CustomResourceOptions
            {
                ReplaceOnChanges = { "immutableStorageWithVersioning" },
                DeleteBeforeReplace = true,
                Parent = storageAccount,
                Aliases = { new Alias
                {
                    NoParent = true
                } }
            });

            var key = new Key(organization, new KeyArgs
            {
                KeyName = organization,
                ResourceGroupName = args.ResourceGroupName,
                VaultName = vault.Name,
                Tags = args.Tags,
                Properties = new KeyPropertiesArgs
                {
                    KeySize = 2048,
                    Kty = JsonWebKeyType.RSA
                }
            }, new CustomResourceOptions
            {
                Parent = vault,
                Aliases = { new Alias
                {
                    Parent = this
                } }
            });
        }

        StorageAccountName = storageAccount.Name;
        KeyVaultName = vault.Name;

        RegisterOutputs();
    }
}