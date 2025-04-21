using Pulumi;

namespace PulumiSharp;

public abstract class Stack
{
    internal abstract object DoBuild();
}

public abstract class Stack<T> : Stack where T : class
{
    internal override object DoBuild()
    {
        return Build();
    }

    public abstract T Build();
}

public abstract class Stack<TOutput, TConfig>(string name) : Stack<TOutput>
    where TOutput : class
    where TConfig : new()
{
    protected TConfig Config { get; set; } = new Config(nameof(Stack).ToLower()).GetObject<TConfig>(name) ?? new TConfig();
}