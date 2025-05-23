# PulumiSharp
[![Build status](https://dev.azure.com/lkt82/Public/_apis/build/status/PulumiSharp%20CI?branchName=main)](https://dev.azure.com/lkt82/Public/_build/latest?definitionId=2)
[![NuGet version (PulumiSharp)](https://img.shields.io/nuget/v/PulumiSharp.svg?style=flat-square)](https://www.nuget.org/packages/PulumiSharp/)

PulumiSharp helps .Net Developers create C# Pulumi projects with types output contracts

Platform support: [.NET 8.0 and later](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8).

- [Quick start](#tasks-quick-start)

## Quick start

- Create a .NET console app named `Sample` and add a reference to [PulumiSharp](https://www.nuget.org/packages/PulumiSharp).
- Replace the contents in `Program.cs` with:
```c#
using Pulumi;
using PulumiSharp;

return await new PulumiRunner().RunAsync(() =>
{
    return new SampleOutput("Hello world");
}, args);

[PulumiProject("PulumiSharp.Sample")]
public record SampleOutput(Output<string> Value);
```
Create a Pulumi.yaml in the project with the content
```yaml
name: PulumiSharp.Sample
runtime: dotnet
description: PulumiSharp Sample
backend:
  url: file://~ 
```
Create a launchSettings.json in the project with the content
```yaml
{
  "profiles": {
    "Default": {
      "commandName": "Project"
    },
    "Pulumi": {
      "commandName": "Project",
      "commandLineArgs": "up",
      "workingDirectory": "."
    }
  }
}
```

Run pulumi stack init from the project folder and type in your stack name


- Run the app using the Pulumi launch profile
