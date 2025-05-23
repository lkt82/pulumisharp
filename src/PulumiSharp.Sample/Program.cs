using PulumiSharp;
using Pulumi;
using System.Collections.Immutable;

//return await new PulumiRunner().RunAsync(() =>
//{
//    var sample = new Sample("sample");

//    return new SampleAOutput(sample.Test);
//}, args);
[assembly: PulumiProject("PulumiSharp.Sample")]

return await new PulumiRunner().RunAsync(() =>
{
    //Debugger.Launch();
    //var sampleOutputStackReference = StackReference<SampleOutput>.Builder.WithStackConfig().Build();

    //return new Dictionary<int, object?>
    //{
    //    { 1, sampleOutputStackReference.Output.ArrayDictionaryOutput }
    //};

    //return "jobs done";

    return new SampleOutput(
        NullOutput: null,
        StringOutput: Output.Create("test"),
        ObjectOutput: Output.Create(new DtoObject("test", 1)),
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

    //return null;

}, args);

public record DtoObject(string Test,int Test2);

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
    Output<ImmutableArray<DtoObject>> ArrayObjectOutput,
    Output<ImmutableArray<int>> ArrayIntOutput,
    Output<ImmutableDictionary<double, double>> DictionaryOutput,
    Output<ImmutableArray<ImmutableDictionary<string, ImmutableDictionary<string, double>>>> ArrayDictionaryOutput
    );