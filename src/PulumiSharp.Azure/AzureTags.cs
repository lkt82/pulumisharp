using System.Collections.ObjectModel;
using Pulumi;

namespace PulumiSharp.Azure;

public class AzureTags : ReadOnlyDictionary<string, string>
{
    public AzureTags() : base(new Dictionary<string, string>
    {
        {"hidden-title",Deployment.Instance.StackName},
        {"Managed-By","Pulumi"}
    })
    {
    }

    public static implicit operator InputMap<string>(AzureTags tags) => Output.Create((IDictionary<string, string>)tags);
}