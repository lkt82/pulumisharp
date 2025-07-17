using Pulumi;
using System.Reflection;

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

public abstract class Stack<TOutput, TConfig> : Stack<TOutput>
    where TOutput : class
    where TConfig : new()
{
    protected Stack()
    {
        var configNameAttribute = typeof(TConfig).GetCustomAttribute<ConfigAttribute>();

        if (configNameAttribute == null)
        {
            throw new InvalidOperationException($"missing {nameof(ConfigAttribute)}");
        }

        Config = new Config(nameof(Stack).ToLower()).GetObject<TConfig>(configNameAttribute.Key) ?? new TConfig();
    }

    protected Stack(string name)
    {
        Config = new Config(nameof(Stack).ToLower()).GetObject<TConfig>(name) ?? new TConfig();
    }

    protected TConfig Config { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ConfigAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}