using System.Collections.Concurrent;
using Azure.Security.KeyVault.Secrets;
using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;

namespace PulumiSharp.Azure.KeyVault;

public record KeyVaultSecretArgs(
    Input<string> ResourceGroupName,
    Input<string> VaultName,
    Input<string> Name,
    Input<string> Value
);

public class KeyVaultSecret : ComponentResource
{
    public Output<string> Name { get; set; }

    public Output<string> Id { get; set; }

    public Output<string> Value { get; set; }

    private readonly ConcurrentDictionary<Uri, SecretClient> _clients = new();

    public static KeyVaultSecret Get(string name,Input<string> id, ComponentResourceOptions? componentOptions = null)
    {
        return new KeyVaultSecret(name, id, componentOptions);
    }

    private KeyVaultSecret(string name, Input<string> id, ComponentResourceOptions? componentOptions = null) : base($"PulumiSharp:Azure:{nameof(KeyVaultSecret)}".ToLower(), name,
        componentOptions)
    {
        var identifier = id.Apply(c => new KeyVaultSecretIdentifier(new Uri(c)));

        var client = identifier.Apply(c =>
        {
            return _clients.GetOrAdd(c.VaultUri, f => new SecretClient(f, new AzureNativeCredential()));
        });

        var result = Output.Tuple(client, identifier.Apply(c=> c.Name)).Apply(async c =>
        {
            var result = await c.Item1.GetSecretAsync(c.Item2);

            return result.Value;
        });

        Name = result.Apply(c => c.Name);
        Id = result.Apply(c => c.Id.ToString());

        Value = Output.CreateSecret(result.Apply(c => c.Value));
    }

    public KeyVaultSecret(string name, KeyVaultSecretArgs args,
        ComponentResourceOptions? componentOptions = null) : base($"PulumiSharp:Azure:{nameof(KeyVaultSecret)}".ToLower(), name,
        componentOptions)
    {
        var secret = new Secret($"{name}", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroupName,
                VaultName = args.VaultName,
                SecretName = args.Name,
                Properties = new SecretPropertiesArgs
                {
                    Value = args.Value
                }
            },
            new()
            {
                Parent = this
            });

        Name = secret.Name;
        Id = secret.Id;
       
        Value = Output.CreateSecret((Output<string>)args.Value);

        RegisterOutputs();
    }
}