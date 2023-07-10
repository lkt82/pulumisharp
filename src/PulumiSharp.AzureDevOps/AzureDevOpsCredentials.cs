using System.Text.Json;
using System.Text.Json.Serialization;

namespace PulumiSharp.AzureDevOps;

public class AzureDevOpsCredentials
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    private static readonly Lazy<AzureDevOpsCredentials?> Credentials = new(() =>
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure-devops", "credentials.json ");
        return File.Exists(path) ? JsonSerializer.Deserialize<AzureDevOpsCredentials>(File.OpenRead(path)) : null;
    });

    public static AzureDevOpsCredentials? Current => Credentials.Value;
}