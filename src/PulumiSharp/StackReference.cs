using Pulumi;

namespace PulumiSharp;

public enum StackReferenceKind
{
    Plain, FromConfig, Split, Combine
}

public class MissingStackReferenceConfigurationEntryException : ArgumentNullException
{
    public MissingStackReferenceConfigurationEntryException() : base() { }
    public MissingStackReferenceConfigurationEntryException(string message) : base(message) { }
    public MissingStackReferenceConfigurationEntryException(string message, Exception innerException) : base(message, innerException) { }
    public MissingStackReferenceConfigurationEntryException(string? paramName, string? message) : base(paramName, message)
    {
    }
}

public class StackReference<TOutputType, TReferenceType> : StackReference
{
    public TOutputType Output { get; }

    public StackReference(string name, string prefix, StackReferenceKind? kind, string? subName) : base(MakeName(name, prefix, kind ?? StackReferenceKind.Plain, subName), null, null)
    {
        Output = this.Get<TOutputType>();
    }

    private static string MakeName(string name, string prefix, StackReferenceKind kind, string? subName)
    {
        return prefix + kind switch
        {
            StackReferenceKind.Plain => name,
            StackReferenceKind.FromConfig => GetAndRequireNameFromConfig(name),
            StackReferenceKind.Split => SplitCompoundStackName(name),
            StackReferenceKind.Combine => CombineCompoundStackName(name, subName!),
            _ => throw new ArgumentException("Unknown kind")
        };
    }

    public static string SplitCompoundStackName(string stackName)
    {
        var index = stackName.IndexOf('.');
        if(index != -1)
            return stackName.Remove(index);
        
        throw new ArgumentException("'stackName' was not a compound stackname. It did not contain a dot.", stackName);
    }

    private static string CombineCompoundStackName(string stackName, string subName)
    {
        return $"{stackName}.{subName}";
    }

    public static string? GetNameFromConfig(string configName)
    {
        var configPrefix = typeof(TReferenceType).Name.ToLower();
        var stackName = new Config(configPrefix).Get(configName);
        return stackName;        
    }

    public static string GetAndRequireNameFromConfig(string configName)
    {
        var stackName = GetNameFromConfig(configName);

        if (stackName != null) return stackName;

        var configPrefix = typeof(TReferenceType).Name.ToLower();

        throw new MissingStackReferenceConfigurationEntryException($"{configPrefix}:{configName}", "the configuration value for the stackreference name may not be null");
    }

    public static string? GetOptionalNameFromConfig(string configName)
    {
        var configPrefix = typeof(TReferenceType).Name.ToLower();
        var stackName = new Config(configPrefix).Get(configName);
        return stackName;
    }
}