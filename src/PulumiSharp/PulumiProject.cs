using JetBrains.Annotations;

namespace PulumiSharp;

[UsedImplicitly]
public class PulumiProject
{
    public string Name { get; set; } = null!;

    public ProjectBackend? Backend { get; set; }
}