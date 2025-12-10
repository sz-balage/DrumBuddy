using System.Security.Claims;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.Endpoint.Endpoints;

public static class SheetEndpoints
{
    public static void MapSheetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sheets")
            .WithTags("Sheets")
            .RequireAuthorization();

        group.MapGet("/summary", GetSheetSummaries)
            .WithName("GetSheets")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetSheet)
            .WithName("GetSheet")
            .WithOpenApi();

        group.MapPost("/", CreateSheet)
            .WithName("CreateSheet")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateSheet)
            .WithName("UpdateSheet")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteSheet)
            .WithName("DeleteSheet")
            .WithOpenApi();
    }
    
    private static async Task<IResult> GetSheetSummaries(
        DrumBuddyDbContext context,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var summaries = await context.Sheets
            .Where(s => s.UserId == userId)
            .Select(s => new SheetSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                UpdatedAt = s.UpdatedAt,
                Tempo = s.Tempo
            })
            .ToListAsync();

        return Results.Ok(summaries);
    }

    private static async Task<IResult> GetSheet(
        Guid id,
        DrumBuddyDbContext context,
        SerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var record = await context.Sheets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (record is null)
            return Results.NotFound();

        var measures = serializationService.DeserializeMeasurementData(record.MeasureBytes);

        var result = new Sheet(record.Tempo, measures, record.Name, record.Description, record.Id)
        {
            IsSyncEnabled = true
        };

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateSheet(
        CreateSheetRequest request,
        SheetRepository repository,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            await repository.SaveSheetAsync(request.Sheet, userId);
            return Results.Created($"/api/sheets/{request.Sheet.Id}", request.Sheet);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> UpdateSheet(
        Guid id,
        UpdateSheetRequest request,
        SheetRepository repository,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            await repository.UpdateSheetAsync(request.Sheet, request.Sheet.UpdatedAt, userId);
            return Results.Ok();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { errors = new[] { ex.Message } });
        }
    }


    private static async Task<IResult> DeleteSheet(
        Guid id,
        DrumBuddyDbContext context,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var record = await context.Sheets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (record is null)
            return Results.NotFound();

        context.Sheets.Remove(record);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}