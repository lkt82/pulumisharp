﻿using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.Random;
using PulumiAzureNativeStorage = Pulumi.AzureNative.Storage.V20220901;

namespace PulumiSharp.Azure.Backend;

public record AzurePulumiBackendArgs(Input<string> ResourceGroupName, InputMap<string> Tags);

public class AzurePulumiBackend : ComponentResource
{
    private readonly Config _config = new(nameof(AzurePulumiBackend).ToLower());

    private List<string> Organizations => _config.RequireObject<List<string>>(nameof(Organizations).ToLower());

    public Output<string> StorageAccountName { get; set; }

    public Output<string> KeyVaultName { get; set; }

    public AzurePulumiBackend(string name, AzurePulumiBackendArgs args) : base(nameof(AzurePulumiBackend), name, new ComponentResourceOptions
    {
        Aliases = { 
            new Alias
            {
               Name = "PulumiService"
            }
        }
    })
    {
        var id = new RandomId("storageaccount", new RandomIdArgs
        {
            ByteLength = 4,
            Prefix = "pulumi"
        }, new()
        {
            Parent = this,
            Aliases = { new Alias { NoParent = true } }
        });

        var storageAccount = new PulumiAzureNativeStorage.StorageAccount("storageaccount", new PulumiAzureNativeStorage.StorageAccountArgs
        {
            AccountName = id.Hex.Apply(c => c.ToLower()),
            ResourceGroupName = args.ResourceGroupName,
            Tags = args.Tags,
            Kind = PulumiAzureNativeStorage.Kind.StorageV2,
            AllowSharedKeyAccess = false,
            AllowBlobPublicAccess = false,
            MinimumTlsVersion = PulumiAzureNativeStorage.MinimumTlsVersion.TLS1_2,
            ImmutableStorageWithVersioning = new PulumiAzureNativeStorage.Inputs.ImmutableStorageAccountArgs
            {
                Enabled = false
            },

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

        var keyVault = new AzureKeyVault("pulumi", new AzureKeyVaultArgs
        (
            ResourceGroupName: args.ResourceGroupName,
            Tags: args.Tags
        ),new()
        {
            Parent = this,
            Aliases = { new Alias { NoParent = true} }
        });

        foreach (var organization in Organizations)
        {
            var blobContainer = new PulumiAzureNativeStorage.BlobContainer(organization, new PulumiAzureNativeStorage.BlobContainerArgs
            {
                AccountName = storageAccount.Name,
                ResourceGroupName = args.ResourceGroupName,
                PublicAccess = PulumiAzureNativeStorage.PublicAccess.None,
                ContainerName = organization,
                ImmutableStorageWithVersioning = new PulumiAzureNativeStorage.Inputs.ImmutableStorageWithVersioningArgs
                {
                    Enabled = false
                }

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
                VaultName = keyVault.Name,
                Tags = args.Tags,
                Properties = new KeyPropertiesArgs
                {
                    KeySize = 2048,
                    Kty = JsonWebKeyType.RSA
                }
            }, new CustomResourceOptions
            {
                Parent = keyVault,
                Aliases = { new Alias
                {
                    Parent = this
                } }
            });
        }

        StorageAccountName = storageAccount.Name;
        KeyVaultName = keyVault.Name;

        RegisterOutputs();
    }
}