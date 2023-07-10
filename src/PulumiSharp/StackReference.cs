using Pulumi;

namespace PulumiSharp;

public class StackReference<T> : StackReference
{
    public StackReference(string name, StackReferenceArgs? args = null, CustomResourceOptions? options = null) : base(name, args, options)
    {
        Output = this.Get<T>();
    }

    public T Output { get; }
}