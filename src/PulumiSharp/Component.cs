using Pulumi;
using PulumiSharp.Reflection;

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

public abstract class Component<TType, TArgs, TConfig>(
    string name,
    TArgs args,
    ComponentResourceOptions? componentOptions = null)
    : Component<TType, TArgs>(name, args, componentOptions)
    where TConfig : new()
{
    protected TConfig Config { get; set; } = (TConfig?)typeof(TConfig).Accept(new ConfigVisitor()) ?? new TConfig();
}