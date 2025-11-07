using System.Security.Claims;
using DrumBuddy.Core.Models;
using DrumBuddy.Endpoint.Data;
using DrumBuddy.Endpoint.Models;
using DrumBuddy.Endpoint.Services;
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

        group.MapGet("/{name}", GetSheet)
            .WithName("GetSheet")
            .WithOpenApi();

        group.MapPost("/", CreateSheet)
            .WithName("CreateSheet")
            .WithOpenApi();

        group.MapPut("/{name}", UpdateSheet)
            .WithName("UpdateSheet")
            .WithOpenApi();

        group.MapDelete("/{name}", DeleteSheet)
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

        var sheets = await context.Sheets
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var result = sheets
            .Select(s => serializationService.DeserializeSheet(s.ContentBytes))
            .ToList();

        return Results.Ok(result);
    }

    private static async Task<IResult> GetSheet(
        string name,
        DrumBuddyDbContext context,
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var sheetOnServer = await context.Sheets
            .FirstOrDefaultAsync(s => s.Name == name && s.UserId == userId);

        if (sheetOnServer is null)
            return Results.NotFound();

        var sheet = serializationService.DeserializeSheet(sheetOnServer.ContentBytes);
        return Results.Ok(sheet);
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

        // Serialize the sheet to protobuf
        var contentBytes = serializationService.SerializeSheet(request.Content);

        var sheet = new SheetOnServer
        {
            ContentBytes = contentBytes,
            Name = request.Content.Name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            context.Sheets.Add(sheet);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            return Results.Conflict(new { message = "You already have a sheet with that name" });
        }

        return Results.Created();
    }

    private static async Task<IResult> UpdateSheet(
        string name,
        UpdateSheetRequest request,
        DrumBuddyDbContext context,
        SheetProtobufSerializationService serializationService,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var sheet = await context.Sheets
            .FirstOrDefaultAsync(s => s.Name == name && s.UserId == userId);

        if (sheet is null)
            return Results.NotFound();

        // Serialize the updated sheet
        var contentBytes = serializationService.SerializeSheet(request.Content);
        
        sheet.ContentBytes = contentBytes;
        sheet.Name = request.Content.Name;
        sheet.UpdatedAt = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            return Results.Conflict(new { message = "A sheet with that name already exists" });
        }

        return Results.Ok(request.Content);
    }

    private static async Task<IResult> DeleteSheet(
        string name,
        DrumBuddyDbContext context,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var sheet = await context.Sheets
            .FirstOrDefaultAsync(s => s.Name == name && s.UserId == userId);

        if (sheet is null)
            return Results.NotFound();

        context.Sheets.Remove(sheet);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
