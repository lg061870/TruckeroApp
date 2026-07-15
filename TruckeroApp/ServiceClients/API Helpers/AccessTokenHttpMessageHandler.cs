using System.Net.Http.Headers;
using TruckeroApp.Interfaces;

namespace TruckeroApp.ServiceClients.ApiHelpers;

public sealed class AccessTokenHttpMessageHandler : DelegatingHandler
{
    private readonly IAuthSessionContext _session;

    public AccessTokenHttpMessageHandler(IAuthSessionContext session)
    {
        _session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null && !string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
