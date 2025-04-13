using Pulumi.Automation;

namespace PulumiSharp;

public static class WorkspaceOptionsFactory
{
    public static InlineProgramArgs CreateInline(string projectName, string? stack, PulumiFn program)
    {
        var localWorkspaceOptions = new InlineProgramArgs(projectName, stack ?? string.Empty, program)
        {
            StackSettings = null
        };

        var profile = PulumiContext.GetProfile<AzurePulumiProfile>();

        var storageAccount = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT") ?? profile?.StorageAccountName ?? throw new Exception("storageAccount not found");
        var keyVault = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT") ?? profile?.KeyVaultName ?? throw new Exception("keyVault not found");
        var organization = Environment.GetEnvironmentVariable("PULUMI_ORGANIZATION") ?? profile?.Organization ?? throw new Exception("organization not found");

        var secretsProvider = $"azurekeyvault://{keyVault}.vault.azure.net/keys/{organization}";

        localWorkspaceOptions.EnvironmentVariables = new Dictionary<string, string?>
        {
            ["PULUMI_SELF_MANAGED_STATE_GZIP"] = "true",
            ["AZURE_STORAGE_ACCOUNT"] = storageAccount,
            ["AZURE_KEY_VAULT"] = keyVault,
            ["PULUMI_ORGANIZATION"] = organization
        };
        if (Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") == null)
        {
            localWorkspaceOptions.EnvironmentVariables["AZURE_KEYVAULT_AUTH_VIA_CLI"] = "true";
        }
        localWorkspaceOptions.SecretsProvider = secretsProvider;

        if (!string.IsNullOrEmpty(stack))
        {
            localWorkspaceOptions.StackSettings = new Dictionary<string, StackSettings>();
            localWorkspaceOptions.StackSettings[stack] = new StackSettings
            {
                SecretsProvider = secretsProvider
            };
        }

        return localWorkspaceOptions;
    }

    public static InlineProgramArgs CreateLocalInline(string projectName, string? stack, PulumiFn program)
    {
        var localWorkspaceOptions = CreateInline(projectName, stack, program);
        localWorkspaceOptions.ProjectSettings!.Main = null;
        localWorkspaceOptions.WorkDir = Directory.GetCurrentDirectory();

        return localWorkspaceOptions;
    }
}