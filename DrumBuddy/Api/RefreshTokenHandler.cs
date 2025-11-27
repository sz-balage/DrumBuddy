using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DrumBuddy.Api.Refit;
using DrumBuddy.IO.Models;

namespace DrumBuddy.Api;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly IAuthApi _authApi;
    private readonly UserService _userService;

    public RefreshTokenHandler(UserService userService, IAuthApi authApi)
    {
        _userService = userService;
        _authApi = authApi;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized &&
            !string.IsNullOrEmpty(_userService.RefreshToken))
        {
            var refreshResponse =
                await _authApi.RefreshAsync(new AuthRequests.RefreshRequest(_userService.RefreshToken));

            if (refreshResponse != null)
            {
                await _userService.SetToken(
                    refreshResponse.AccessToken,
                    refreshResponse.RefreshToken,
                    _userService.UserName!,
                    _userService.Email!,
                    _userService.UserId!);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", refreshResponse.AccessToken);

                return await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}