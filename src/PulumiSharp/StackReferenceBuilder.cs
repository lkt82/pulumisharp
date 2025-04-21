using System.Reflection;
using Pulumi;

namespace PulumiSharp;

public class StackReferenceBuilder<T> where T : class
{
    private string? _stack;
    private string _name = typeof(T).Name.ToLower();
    private string? _organization;
    private string? _project;

    public StackReferenceBuilder<T> WithName(string name)
    {
        _name = name;

        return this;
    }

    public StackReferenceBuilder<T> WithStack(string stack)
    {
        _stack = stack;

        return this;
    }

    public StackReferenceBuilder<T> WithOrganization(string organization)
    {
        _organization = organization;

        return this;
    }

    public StackReferenceBuilder<T> WithProject(string project)
    {
        _project = project;
            
        return this;
    }

    public StackReferenceBuilder<T> WithStackConfig(string? configName=null, string? configKey = null)
    {
        _stack = new Config(configName ?? _name).Get(configKey ?? "stack");

        return this;
    }

    public StackReference<T> Build()
    {
        _project ??= typeof(T).GetCustomAttribute<PulumiProjectAttribute>()?.Name;

        if (_organization == null && _project == null && _stack == null)
        {
            return new StackReference<T>(_name);
        }

        return new StackReference<T>(_name, new StackReferenceArgs
        {
            Name = Output.Format($"{_organization?? Deployment.Instance.GetOrganizationName()}/{_project}/{_stack?? Deployment.Instance.StackName}")
        });
    }
}