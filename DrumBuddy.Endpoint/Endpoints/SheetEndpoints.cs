using System.Security.Claims;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.Endpoint.Models;
using DrumBuddy.Endpoint.Services;
using DrumBuddy.IO.Data;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.Endpoint.Endpoints;

public static class SheetEndpoints
{
    public static void MapSheetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sheets")
            .WithTags("Sheets")
            .RequireAuthorization();

        group.MapGet("/", GetSheets)
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

    private static async Task<IResult> GetSheets(
        DrumBuddyDbContext context,
        SerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var records = await context.Sheets
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var sheets = records.Select(record =>
        {
            var measures = serializationService.DeserializeMeasurementData(record.MeasureBytes);
            return new Sheet(record.Tempo, measures, record.Name, record.Description, record.Id)
            {
                LastSyncedAt = record.LastSyncedAt,
                IsSyncEnabled = true 
            };
        }).ToList();

        return Results.Ok(sheets);
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

        // Restore Guid and sync info
        var result = new Sheet(record.Tempo, measures, record.Name, record.Description, record.Id)
        {
            LastSyncedAt = record.LastSyncedAt,
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
            // Pass userId to repository, don't set on Sheet
            await repository.UpdateSheetAsync(request.Sheet, userId);
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