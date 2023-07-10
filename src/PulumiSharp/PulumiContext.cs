using System.Text.Json;

namespace PulumiSharp;

public static class PulumiContext
{
    private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".pulumi\\profiles.json");

    private static readonly object InstanceLock = new();
    private static readonly AsyncLocal<PulumiProfiles?> InstanceAsyncLocal = new();

    private static PulumiProfiles? Instance
    {
        get
        {
            lock (InstanceLock)
            {
                return InstanceAsyncLocal.Value;
            }
        }
        set
        {
            lock (InstanceLock)
            {
                InstanceAsyncLocal.Value = value;
            }
        }
    }

    public static async Task<PulumiProfiles?> GetProfilesAsync()
    {
        if (Instance == null && File.Exists(Path))
        {
            Instance = JsonSerializer.Deserialize<PulumiProfiles>(await File.ReadAllTextAsync(Path),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return Instance;
    }

    public static PulumiProfiles? GetProfiles()
    {
        if (Instance == null && File.Exists(Path))
        {
            Instance = JsonSerializer.Deserialize<PulumiProfiles>(File.ReadAllText(Path),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        return Instance;
    }

    public static PulumiProfile? GetProfile()
    {
        var profiles = GetProfiles();

        return profiles?.Current == null ? null : profiles.Profiles[profiles.Current!];
    }

    public static async Task SaveProfilesAsync(PulumiProfiles profiles)
    {
        await File.WriteAllTextAsync(Path, JsonSerializer.Serialize(profiles, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));

        Instance = profiles;
    }
}