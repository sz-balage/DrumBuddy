using System.Security.Claims;
using DrumBuddy.Core.Models;
using DrumBuddy.Endpoint.Models;
using DrumBuddy.Endpoint.Services;
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
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var records = await context.Sheets
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        // Deserialize records to Sheets
        var sheets = records.Select(record =>
        {
            var measures = serializationService.DeserializeSheet(record.MeasureBytes);
            // Restore Guid and sync info from database
            return new Sheet(record.Tempo, measures, record.Name, record.Description, record.Id)
            {
                LastSyncedAt = record.LastSyncedAt,
                IsSyncEnabled = true // Server sheets are always synced
            };
        }).ToList();

        return Results.Ok(sheets);
    }

    private static async Task<IResult> GetSheet(
        Guid id,
        DrumBuddyDbContext context,
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var record = await context.Sheets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (record is null)
            return Results.NotFound();

        var measures = serializationService.DeserializeSheet(record.MeasureBytes);
        
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
        DrumBuddyDbContext context,
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var measureBytes = serializationService.SerializeSheet(request.Sheet.Measures);

        var record = new SheetRecord
        {
            Id = request.Sheet.Id, // Use Guid from sheet
            MeasureBytes = measureBytes,
            Name = request.Sheet.Name,
            Description = request.Sheet.Description,
            Tempo = request.Sheet.Tempo.Value,
            UserId = userId,
            LastSyncedAt = DateTime.UtcNow
        };

        try
        {
            context.Sheets.Add(record);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            return Results.Conflict(new { errors = new[] { "You already have a sheet with that name" } });
        }
        catch (DbUpdateException ex)
        {
            return Results.BadRequest(new { errors = new[] { ex.InnerException?.Message ?? "Database error" } });
        }

        return Results.Created($"/api/sheets/{record.Id}", request.Sheet);
    }

    private static async Task<IResult> UpdateSheet(
        Guid id,
        UpdateSheetRequest request,
        DrumBuddyDbContext context,
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var record = await context.Sheets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (record is null)
            return Results.NotFound();

        var measureBytes = serializationService.SerializeSheet(request.Sheet.Measures);
        
        record.MeasureBytes = measureBytes;
        record.Name = request.Sheet.Name;
        record.Description = request.Sheet.Description;
        record.Tempo = request.Sheet.Tempo.Value;
        record.LastSyncedAt = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            return Results.Conflict(new { errors = new[] { "A sheet with that name already exists" } });
        }
        catch (DbUpdateException ex)
        {
            return Results.BadRequest(new { errors = new[] { ex.InnerException?.Message ?? "Database error" } });
        }
        return Results.Ok();
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
