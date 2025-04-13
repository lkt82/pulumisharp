using System.Collections.Immutable;
using System.Text.Json;
using JetBrains.Annotations;
using Pulumi;

namespace PulumiSharp;

[UsedImplicitly]
public static class OutputExtensions
{
    public static Output<string> JsonSerialize<T>(this Output<T> output, JsonSerializerOptions? options = null)
        where T : notnull
    {
        return Output.JsonSerialize(output, options);
    }

    public static Output<T> JsonDeserialize<T>(this Output<string> output, JsonSerializerOptions? options = null)
        where T : notnull
    {
        return Output.JsonDeserialize<T>(output, options);
    }

    public static Output<ImmutableArray<string>> JsonSerialize<T>(this Output<ImmutableArray<T>> output, JsonSerializerOptions? options = null)
        where T : notnull
    {
        return output.Apply(c =>
        {
            return Output.All(c.Select(value => Output.JsonSerialize(Output.Create(value), options)));
        });
    }

    public static Output<ImmutableArray<T>> JsonDeserialize<T>(this Output<ImmutableArray<string>> output, JsonSerializerOptions? options = null)
        where T : notnull
    {
        return output.Apply(c =>
        {
            return Output.All(c.Select(value => Output.JsonDeserialize<T>(Output.Create(value), options)));
        });
    }

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

    public static Output<ImmutableDictionary<T1, T2>> ToOutput<T1, T2>(this IDictionary<T1, Output<T2>> output) where T1 : notnull
    {
        return Output.All(output.Values).Apply(c =>
        {
            var dictionary = new Dictionary<T1, T2>();
            var keys = output.Keys.ToArray();
            for (var i = 0; i < c.Length; i++)
            {
                dictionary.Add(keys[i], c[i]);
            }

            return dictionary.ToImmutableDictionary();
        });
    }
}