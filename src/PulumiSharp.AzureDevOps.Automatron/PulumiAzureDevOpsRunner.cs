using Automatron.AzureDevOps;

namespace PulumiSharp.AzureDevOps.Automatron;

public class PulumiAzureDevOpsRunner : AzureDevOpsRunner<PulumiAzureDevOpsCommand>
{
    public PulumiAzureDevOpsRunner()
    {
        this.UsePulumi();
    }
}