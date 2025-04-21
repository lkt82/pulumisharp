namespace PulumiSharp;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
public class PulumiProjectAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}