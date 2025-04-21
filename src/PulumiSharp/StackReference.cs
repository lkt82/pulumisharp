using Pulumi;

namespace PulumiSharp;

public class StackReference<TOutput> : StackReference
    where TOutput : class
{
    public StackReference(string name,
        StackReferenceArgs? args = null,
        CustomResourceOptions? options = null) : base(name, args, options)
    {
        Output = OutputSerializer.Deserialize<TOutput>(Outputs);
    }

    public TOutput Output { get; }

    public static StackReferenceBuilder<TOutput> Builder = new();
}