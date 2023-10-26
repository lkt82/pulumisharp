using Pulumi;

namespace PulumiSharp;

public abstract class Component<TType> : ComponentResource
{
    protected string Name => GetResourceName();
    protected string Type => GetResourceType();

    protected Component(string name, ComponentResourceOptions? componentOptions = null) : base(typeof(TType).Namespace != null ? typeof(TType).Namespace!.ToLower().Replace(".",":")+":"+ typeof(TType).Name : typeof(TType).Name, name, componentOptions)
    {

    }
}

public abstract class Component<TType,TArgs> : Component<TType>
{
    protected TArgs Args { get; init; }

    protected Component(string name, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, componentOptions)
    {
        Args = args;
    }
}


public abstract class Component<TType, TArgs, TConfig> : Component<TType, TArgs>
{
    protected TConfig Config { get; init; }

    protected Component(string name, TArgs args, ComponentResourceOptions? componentOptions = null) : base(name, args, componentOptions)
    {
        Config = new Config(Type).RequireObject<TConfig>(name);
    }
}