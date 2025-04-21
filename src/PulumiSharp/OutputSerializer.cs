using System.Collections.Immutable;
using Pulumi;
using PulumiSharp.Reflection;

namespace PulumiSharp;

internal static class OutputSerializer
{
    public static IDictionary<string, object?> Serialize<TOutput>(TOutput? output) where TOutput : class
    {
        return typeof(TOutput).Accept(new OutputVisitor(output))!;
    }

    public static TOutput Deserialize<TOutput>(Output<ImmutableDictionary<string, object>> output) where TOutput : class
    {
        return typeof(TOutput).Accept(new OutputVisitor<TOutput>(output))!;
    }

}