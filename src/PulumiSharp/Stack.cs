using Pulumi;

namespace PulumiSharp;

public abstract class Stack
{
    internal abstract Dictionary<string, object?> DoBuild();
}

public abstract class Stack<T> : Stack where T : class
{
    internal override Dictionary<string, object?> DoBuild()
    {
        return Build().ToDictionary();
    }

    public abstract T Build();
}

public abstract class Stack<TOutput, TConfig>(string name) : Stack<TOutput>
    where TOutput : class
    where TConfig : new()
{
    protected TConfig Config { get; set; } = new Config(nameof(Stack).ToLower()).GetObject<TConfig>(name) ?? new TConfig();
}