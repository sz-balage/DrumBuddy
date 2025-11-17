using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DrumBuddy.Api;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync(ct);
            Console.WriteLine("REQUEST BODY:\n" + body);
        }
        return await base.SendAsync(request, ct);
    }
}
