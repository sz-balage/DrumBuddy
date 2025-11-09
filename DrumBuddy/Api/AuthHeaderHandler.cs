using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DrumBuddy.Api;

namespace DrumBuddy.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly UserService _userService;

    public AuthHeaderHandler(UserService userService)
    {
        _userService = userService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _userService.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}