using DrumBuddy.Endpoint.Endpoints;
using Scalar.AspNetCore;

namespace DrumBuddy.Endpoint.Extensions;

public static class WebApplicationExtensions
{
    public static void UseApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void MapApplicationEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapSheetEndpoints();
    }
}