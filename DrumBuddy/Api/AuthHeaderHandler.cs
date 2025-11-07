using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DrumBuddy.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;

    public AuthHeaderHandler(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Get token from service
        var token = await _tokenService.GetTokenAsync();

        // Add Authorization header if token exists
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}