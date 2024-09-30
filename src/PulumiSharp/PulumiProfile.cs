using System.Text.Json.Serialization;

namespace PulumiSharp;

[JsonConverter(typeof(PulumiProfileJsonConverter))]
public abstract class PulumiProfile
{
    public int Type { get; set; } = 1;

    public string Organization { get; set; } = null!;
}