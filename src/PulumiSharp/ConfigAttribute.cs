namespace PulumiSharp;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class ConfigAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Key { get; set; }

    public bool Secret { get; set; } = false;

    public bool Required { get; set; } = false;
}