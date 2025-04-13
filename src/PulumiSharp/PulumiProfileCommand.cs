using CommandDotNet;
using Spectre.Console;
using CommandContext = CommandDotNet.CommandContext;
using static SimpleExec.Command;

namespace PulumiSharp;

[Subcommand]
[Command("profile")]
internal class PulumiProfileCommand(IAnsiConsole ansiConsole, CommandContext commandContext)
    : PulumiCommandBase(ansiConsole, commandContext)
{
    private readonly IAnsiConsole _ansiConsole = ansiConsole;

    [Command("ls")]
    public async Task<int> List()
    {
        var profiles = await PulumiContext.GetProfilesAsync();

        if (profiles == null)
        {
            _ansiConsole.MarkupLine("[red]Profile is not is configured[/]");
            return 1;
        }

        foreach (var account in profiles.Profiles)
        {
            _ansiConsole.Write(account.Key);
        }

        return 0;
    }

    [Command("rm")]
    public async Task<int> Remove(string profile)
    {
        var profiles = await PulumiContext.GetProfilesAsync();

        if (profiles == null)
        {
            _ansiConsole.MarkupLine($"[red]Profile {profile} is not is configured[/]");
            return 1;
        }

        profiles.Profiles.Remove(profile);

        if (profiles.Current == profile)
        {
            profiles.Current = null;
        }

        await PulumiContext.SaveProfilesAsync(profiles);

        return 0;
    }

    [Command("unselect")]
    public async Task<int> Unselect(string profile)
    {
        var profiles = await PulumiContext.GetProfilesAsync();

        if (profiles == null)
        {
            _ansiConsole.MarkupLine($"[red]Profile {profile} is not is configured[/]");
            return 1;
        }

        profiles.Current = null;

        await PulumiContext.SaveProfilesAsync(profiles);

        return 0;
    }

    [Command("select")]
    public async Task<int> Select(string profile)
    {
        var profiles = await PulumiContext.GetProfilesAsync();
        if (profiles == null)
        {
            _ansiConsole.MarkupLine($"[red]Profile {profile} is not is configured[/]");
            return 1;
        }

        profiles.Current = profile;

        var pulumiAccount = profiles.Profiles[profile];

        await PulumiContext.SaveProfilesAsync(profiles);

        if (pulumiAccount is AzurePulumiProfile azurePulumiProfile)
        {
            await RunAsync("az", $"account set --subscription {azurePulumiProfile.SubscriptionId}");
        }

        return 0;
    }

    [Command("login")]
    public async Task<int> Login(string profile)
    {
        var profiles = await PulumiContext.GetProfilesAsync();
        if (profiles == null)
        {
            _ansiConsole.MarkupLine($"[red]Profile {profile} is not is configured[/]");
            return 1;
        }

        profiles.Current = profile;

        var pulumiAccount = profiles.Profiles[profile];

        if (pulumiAccount is AzurePulumiProfile azurePulumiProfile)
        { 
            await RunAsync("az", "login");
            await RunAsync("az", $"account set --subscription {azurePulumiProfile.SubscriptionId}");
        }

        await PulumiContext.SaveProfilesAsync(profiles);

        return 0;
    }

    [Command("init-azure")]
    public async Task AzureInit(string profile, string organization, string storageAccountName, string keyVaultName, string tenantId, string subscriptionId)
    {
        var profiles = await PulumiContext.GetProfilesAsync();

        if (profiles != null)
        {
            profiles.Current = profile;

            var pulumiAccount = (AzurePulumiProfile)profiles.Profiles[profile];

            pulumiAccount.Organization = organization;
            pulumiAccount.StorageAccountName = storageAccountName;
            pulumiAccount.KeyVaultName = keyVaultName;
            pulumiAccount.TenantId = tenantId;
            pulumiAccount.SubscriptionId = subscriptionId;
        }
        else
        {
            profiles = new PulumiProfiles
            {
                Current = profile,
                Profiles = new Dictionary<string, PulumiProfile>
                {
                    { profile,new AzurePulumiProfile
                        {
                            Organization = organization,
                            StorageAccountName = storageAccountName,
                            KeyVaultName = keyVaultName,
                            TenantId = tenantId,
                            SubscriptionId = subscriptionId
                        }
                    }
                }
            };
        }

        await PulumiContext.SaveProfilesAsync(profiles);
    }
}