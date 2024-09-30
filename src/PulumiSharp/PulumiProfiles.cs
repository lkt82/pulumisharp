namespace PulumiSharp;


public class PulumiProfiles
{
    public string? Current { get; set; }

    public Dictionary<string, PulumiProfile> Profiles { get; set; } = new();
}