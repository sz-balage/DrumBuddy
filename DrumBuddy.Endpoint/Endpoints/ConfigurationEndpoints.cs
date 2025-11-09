using System.Security.Claims;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using DrumBuddy.IO.Storage;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.Endpoint.Endpoints;

public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/config")
            .WithTags("Configuration")
            .RequireAuthorization();

        group.MapGet("/", GetConfiguration)
            .WithName("GetConfiguration")
            .WithOpenApi();

        group.MapPut("/", UpdateConfiguration)
            .WithName("UpdateConfiguration")
            .WithOpenApi();
    }

    private static async Task<IResult> GetConfiguration(
        ConfigurationRepository repository,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var config = await repository.LoadConfigAsync(userId);
        return Results.Ok(config);
    }

    private static async Task<IResult> UpdateConfiguration(
        UpdateConfigurationRequest request,
        ConfigurationRepository repository,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            await repository.SaveConfigAsync(request.Configuration, userId);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { errors = new[] { ex.Message } });
        }
    }
}

