using System.Collections.Immutable;
using System.Text.Json;
using Pulumi;

namespace PulumiSharp;

public static class OutputExtensions
{
    public static Output<ImmutableDictionary<T1, string>> JsonSerialize<T1,T2>(this Output<ImmutableDictionary<T1, Output<T2>>> output, JsonSerializerOptions? options = null) where T1 : notnull
    {
        return output.Apply(outputs =>
        {
            var keys = outputs.Keys.ToArray();

            return Output.All(outputs.Values.Select(value => Output.JsonSerialize(value, options))).Apply(values =>
            {
                var dictionary = new Dictionary<T1, string>();

                for (var i = 0; i < outputs.Count; i++)
                {
                    var key = keys[i];
                    var value = values[i];

                    dictionary.Add(key, value);
                }

                return dictionary.ToImmutableDictionary();
            });
        });
    }

    public static Output<ImmutableDictionary<T1, T2>> JsonDeserialize<T1, T2>(this Output<ImmutableDictionary<T1, string>> output, JsonSerializerOptions? options = null) where T1 : notnull
    {
        return output.Apply(outputs =>
        {
            var keys = outputs.Keys.ToArray();

            return Output.All(outputs.Values.Select(value => Output.JsonDeserialize<T2>((Input<string>)value, options))).Apply(values =>
            {
                var dictionary = new Dictionary<T1, T2>();

                for (var i = 0; i < outputs.Count; i++)
                {
                    var key = keys[i];
                    var value = values[i];

                    dictionary.Add(key, value);
                }

                return dictionary.ToImmutableDictionary();
            });
        });
    }
}