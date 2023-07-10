namespace PulumiSharp;

public class PulumiProfile
{
    public string Organization { get; set; } = null!;

    public string StorageAccountName { get; set; } = null!;

    public string KeyVaultName { get; set; } = null!;

    public string TenantId { get; set; } = null!;

    public string SubscriptionId { get; set; } = null!;
}