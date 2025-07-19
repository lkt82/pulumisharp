using System.Collections.Immutable;
using Pulumi;

namespace PulumiSharp.Sample;

public static class Constants
{
    public const string ConfigName = "prop";
}

//[Config(Name = Constants.ConfigName)]
public class SampleConfig
{
    [Config(Key = "secret", Secret = true)]
    public Output<string> Secret { get; set; }

    [Config(Key = "test")]
    public string Test { get; set; }

    //[Config(Key = "json")]
    public SampleJson Json { get; set; }
}

[Config(Name = Constants.ConfigName, Key = "json")]
public class SampleJson
{
    public string String { get; set; }

    public int Int { get; set; }

    public string Secret { get; set; }
}

public record DtoObject(string Test, int Test2);

public record SampleDto(
    string String,
    DtoObject Object,
    ImmutableArray<DtoObject> ArrayObject,
    ImmutableArray<int> ArrayInt,
    ImmutableDictionary<double, double> Dictionary,
    ImmutableArray<ImmutableDictionary<string, ImmutableDictionary<string, double>>> ArrayDictionary
);

[PulumiProject("PulumiSharp.Sample")]
public record SampleOutput(
    Output<string> StringOutput,
    Output<string>? NullOutput,
    Output<DtoObject> ObjectOutput,
    [property: JsonOutput] Output<DtoObject> JsonOutput,
    [property: JsonOutput] Output<ImmutableArray<DtoObject>> JsonArrayObjectOutput,
    Output<ImmutableArray<DtoObject>> ArrayObjectOutput,
    Output<ImmutableArray<int>> ArrayIntOutput,
    Output<ImmutableDictionary<double, double>> DictionaryOutput,
    Output<ImmutableArray<ImmutableDictionary<string, ImmutableDictionary<string, double>>>> ArrayDictionaryOutput
);

public class SampleStack : Stack<SampleOutput, SampleJson>
{
    public override SampleOutput Build()
    {
        //Debugger.Launch();
        //var sampleOutputStackReference = StackReference<SampleOutput>.Builder.WithStackConfig().Build();

        //return new Dictionary<int, object?>
        //{
        //    { 1, sampleOutputStackReference.Output.ArrayDictionaryOutput },
        //    { 2, sampleOutputStackReference.Output.JsonArrayObjectOutput }
        //};

        //return "jobs done";

        var config = Config;

        return new SampleOutput(
            NullOutput: null,
            StringOutput: Output.Create("test"),
            ObjectOutput: Output.Create(new DtoObject("test", 1)),
            JsonOutput: Output.Create(new DtoObject("test", 1)),
            JsonArrayObjectOutput: Output.Create(new[] { new DtoObject("test", 2) }.ToImmutableArray()),
            ArrayObjectOutput: Output.Create(new[] { new DtoObject("test", 2) }.ToImmutableArray()),
            ArrayIntOutput: Output.Create(new[] { 1, 2 }.ToImmutableArray()),
            DictionaryOutput: Output.Create(new Dictionary<double, double> { { 2.2, 1.6 } }.ToImmutableDictionary()),
            ArrayDictionaryOutput: Output.Create(new[]
            {
                new Dictionary<string, ImmutableDictionary<string, double>>
                {
                    { "property",
                        new Dictionary<string, double>
                        {
                            { "property", 1.6 }
                        }.ToImmutableDictionary()
                    }
                }.ToImmutableDictionary()
            }.ToImmutableArray())
        );
    }
}