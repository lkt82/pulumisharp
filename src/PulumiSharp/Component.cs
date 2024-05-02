using Pulumi;

namespace PulumiSharp;

public abstract class Component<TType> : ComponentResource
{
    protected string Name => GetResourceName();
    protected string Type => GetResourceType();

    protected ComponentResourceOptions? Options { get; init; }

    protected Component(string name, ComponentResourceOptions? componentOptions = null) : base(typeof(TType).Namespace != null ? typeof(TType).Namespace!.ToLower().Replace(".", ":") + ":" + typeof(TType).Name : typeof(TType).Name, name, componentOptions)
    {
        Options = componentOptions;
    }
}

public abstract class Component<TType, TArgs> : Component<TType>
{
    protected TArgs Args { get; init; }

    protected Component(string name, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, componentOptions)
    {
        Args = args;
    }
}


public abstract class Component<TType, TArgs, TConfig> : Component<TType, TArgs> where TConfig : new()
{
    protected TConfig Config { get; set; }

    protected Component(string name, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = new Config(Type.ToLower().Replace(":", "-")).GetObject<TConfig>(name) ?? new TConfig();
    }

    protected Component(string name, string configName, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = new Config(configName).GetObject<TConfig>(name) ?? new TConfig();
    }
}