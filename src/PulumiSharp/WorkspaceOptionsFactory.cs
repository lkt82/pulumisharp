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

        var profile = PulumiContext.GetProfile();

        if (profile != null)
        {
            var storageAccount = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT") ?? profile?.StorageAccountName ?? throw new Exception("storageAccount for AzureBlobStackReference not found");

            var secretsProvider = $"azurekeyvault://{profile.KeyVaultName}.vault.azure.net/keys/{profile.Organization}";

            localWorkspaceOptions.EnvironmentVariables = new Dictionary<string, string?>
            {
                ["PULUMI_SELF_MANAGED_STATE_GZIP"] = "true",
                ["AZURE_STORAGE_ACCOUNT"] = storageAccount
            };
            if (Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") == null)
            {
                localWorkspaceOptions.EnvironmentVariables["AZURE_KEYVAULT_AUTH_VIA_CLI"] = "true";
            }
            localWorkspaceOptions.SecretsProvider = secretsProvider;
            localWorkspaceOptions.ProjectSettings!.Backend = new global::Pulumi.Automation.ProjectBackend
            {
                Url = $"azblob://{profile.Organization}/{projectName}"
            };
            if (!string.IsNullOrEmpty(stack))
            {
                localWorkspaceOptions.StackSettings = new Dictionary<string, StackSettings>();
                localWorkspaceOptions.StackSettings[stack] = new StackSettings
                {
                    SecretsProvider = secretsProvider
                };
            }
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