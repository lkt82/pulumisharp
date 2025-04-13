using Pulumi;

namespace PulumiSharp;

public abstract class Component<TType>(string name, ComponentResourceOptions? componentOptions = null)
    : ComponentResource(
        typeof(TType).Namespace != null
            ? typeof(TType).Namespace!.ToLower().Replace(".", ":") + ":" + typeof(TType).Name
            : typeof(TType).Name, name, componentOptions)
{
    protected string Name => GetResourceName();
    protected string Type => GetResourceType();

    protected ComponentResourceOptions? Options { get; init; } = componentOptions;
}

public abstract class Component<TType, TArgs>(
    string name,
    TArgs args,
    ComponentResourceOptions? componentOptions = null)
    : Component<TType>(name, componentOptions)
{
    protected TArgs Args { get; init; } = args;
}

public abstract class Component<TType, TArgs, TConfig> : Component<TType, TArgs> where TConfig : new()
{
    protected TConfig Config { get; set; }

    protected Component(string name, TArgs args, TConfig config,ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = config;
    }

    protected Component(string name, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = new Config(Type.ToLower().Replace(":","-")).GetObject<TConfig>(name)??new TConfig();
    }

    protected Component(string name,string configName, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = new Config(configName).GetObject<TConfig>(name) ?? new TConfig();
    }
}