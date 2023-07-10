using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PulumiSharp.AzureDevOps.Automatron;

[DeploymentJob("Deployment", Environment = "${{Environment}}")]
public abstract class PulumiDeploymentJob
{
    protected readonly LoggingCommands LoggingCommands;
    protected readonly PulumiCli Pulumi;
    protected readonly string Project;
    protected readonly string Stack;

    protected PulumiDeploymentJob(LoggingCommands loggingCommands, PulumiCli pulumi,string project, string stack)
    {
        LoggingCommands = loggingCommands;
        Pulumi = pulumi;
        Project = project;
        Stack = stack;
    }

    protected virtual IDictionary<string, string?> Env
    {
        get
        {
            if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
            {
                return new Dictionary<string, string?>
                {
                    { "PULUMI_SELF_MANAGED_STATE_GZIP", "true" },
                    { "ARM_CLIENT_ID", AzureClientId },
                    { "ARM_CLIENT_SECRET", AzureClientSecret?.GetValue() },
                    { "ARM_TENANT_ID", AzureTenantId }
                };
            }

            return new Dictionary<string, string?>
            {
                { "AZURE_KEYVAULT_AUTH_VIA_CLI", "true"},
                { "PULUMI_SELF_MANAGED_STATE_GZIP", "true" },
                { "AZURE_STORAGE_ACCOUNT", AzureStorageAccount }
            };
        }
    }

    [Variable]
    public Secret? PulumiAccessToken { get; set; }

    [Variable]
    public string? PulumiOrganization { get; set; } = PulumiContext.GetProfile()?.Organization;

    [Variable]
    public Secret? AzureClientSecret { get; set; }

    [Variable]
    public string? AzureClientId { get; set; }

    [Variable]
    public string? AzureTenantId { get; set; }

    [Variable] 
    public string? AzureStorageAccount { get; set; } = PulumiContext.GetProfile()?.StorageAccountName;

    [Variable]
    public string? AzureKeyVault { get; set; } = PulumiContext.GetProfile()?.KeyVaultName;


    [Checkout(CheckoutSource.Self, FetchDepth = 0)]
    [NuGetAuthenticate]
    [Step]
    public virtual async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await LoggingCommands.UpdateBuildNumberAsync(version);
    }

    [Pulumi(DisplayName = "Pulumi Install")]
    [Step(DisplayName = "Pulumi Init",DependsOn = new []{ nameof(Version) })]
    public virtual async Task PulumiInit()
    {
        await Pulumi.RunCommand("plugin install", Project, Env);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(LowerCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var projectSettings = deserializer.Deserialize<ProjectSettings>(File.OpenText(Path.Combine(Project, "Pulumi.yaml")));

        if (projectSettings.Backend?.Url != null && projectSettings.Backend.Url.StartsWith("azblob"))
        {
            var secretsProvider = $"azurekeyvault://{AzureKeyVault}.vault.azure.net/keys/{PulumiOrganization}";

            await Pulumi.RunCommand($"stack select {Stack} -c --secrets-provider=\"{secretsProvider}\"", Project, Env);
        }
        else
        {
            await Pulumi.RunCommand($"stack select {Stack} -c", Project, Env);
        }
    }

    [Step(DisplayName = "Pulumi Preview", DependsOn = new[] { nameof(PulumiInit) })]
    public virtual async Task PulumiPreview()
    {
        await Pulumi.RunCommand("preview", Project, Env);
    }

    [Step(DisplayName = "Pulumi Update", DependsOn = new[] { nameof(PulumiPreview) })]
    public virtual async Task PulumiUpdate()
    {
        await Pulumi.RunCommand("up -f -y", Project, Env);
    }
}