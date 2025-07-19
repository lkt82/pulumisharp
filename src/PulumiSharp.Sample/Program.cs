using PulumiSharp;
using PulumiSharp.Sample;

[assembly: PulumiProject("PulumiSharp.Sample")]

return await new PulumiRunner<SampleStack>().RunAsync(args);