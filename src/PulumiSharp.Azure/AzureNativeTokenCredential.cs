using Azure.Core;
using Pulumi.AzureNative.Authorization;

namespace PulumiSharp.Azure;

public class AzureNativeTokenCredential : TokenCredential
{
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var token = await GetClientToken.InvokeAsync();

        return new AccessToken(token.Token,DateTimeOffset.UtcNow.AddHours(5));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return GetTokenAsync(requestContext, cancellationToken).Result;
    }
}