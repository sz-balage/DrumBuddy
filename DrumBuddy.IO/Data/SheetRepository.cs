using System.Collections.Immutable;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Models;
using Microsoft.EntityFrameworkCore;

namespace DrumBuddy.IO.Data;

public class SheetRepository
{
    private readonly DrumBuddyDbContext _context;
    private readonly SerializationService _serializationService;

    public SheetRepository(DrumBuddyDbContext context, SerializationService serializationService)
    {
        _context = context;
        _serializationService = serializationService;
    }

    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync(string? userId)
    {
        IQueryable<SheetRecord> query = _context.Sheets;

        query = query.Where(s => s.UserId == userId);
        var records = await query
            .OrderBy(s => s.Name)
            .ToListAsync();
        var sheets = records.Select(DeserializeToSheet).ToList();
        return [..sheets];
    }

    public async Task<Sheet?> GetSheetByIdAsync(Guid id, string? userId)
    {
        IQueryable<SheetRecord> query = _context.Sheets;
        query = query.Where(s => s.UserId == userId);
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(s => s.UserId == null || s.UserId == userId);

        var record = await query.FirstOrDefaultAsync(s => s.Id == id);
        return record == null ? null : DeserializeToSheet(record);
    }
    public async Task SaveSheetAsync(SheetDto sheetDto, string? userId)
    {
        var record = new SheetRecord
        {
            Id = sheetDto.Id,
            MeasureBytes = sheetDto.MeasureBytes,
            Name = sheetDto.Name,
            Description = sheetDto.Description,
            Tempo = sheetDto.Tempo,
            UserId = userId,
            UpdatedAt = sheetDto.UpdatedAt
        };

        try
        {
            _context.Sheets.Add(record);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            throw new InvalidOperationException("You already have a sheet with that name", ex);
        }
    }

    public async Task SaveSheetAsync(Sheet sheet, string? userId)
    {
        var measureBytes = _serializationService.SerializeMeasurementData(sheet.Measures);

        var record = new SheetRecord
        {
            Id = sheet.Id,
            MeasureBytes = measureBytes,
            Name = sheet.Name,
            Description = sheet.Description,
            Tempo = sheet.Tempo.Value,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId 
        };

        try
        {
            _context.Sheets.Add(record);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            throw new InvalidOperationException("You already have a sheet with that name", ex);
        }
    }
    public async Task UpdateSheetAsync(SheetDto sheetDto, DateTime updatedAt, string? userId)
    {
        var query = _context.Sheets.AsQueryable();
        
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(s => s.UserId == userId);

        var record = await query.FirstOrDefaultAsync(s => s.Id == sheetDto.Id);

        if (record == null)
            throw new InvalidOperationException($"Sheet with ID {sheetDto.Id} not found");

        record.Name = sheetDto.Name;
        record.Description = sheetDto.Description;
        record.Tempo = sheetDto.Tempo;
        record.MeasureBytes = sheetDto.MeasureBytes;
        record.UpdatedAt = updatedAt;

        try
        {
            _context.Sheets.Update(record);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            throw new InvalidOperationException("A sheet with that name already exists", ex);
        }
    }
    public async Task UpdateSheetAsync(Sheet sheet, DateTime updatedAt, string? userId)
    {
        var query = _context.Sheets.AsQueryable();
        
        query = query.Where(s => s.UserId == userId);


        var record = await query.FirstOrDefaultAsync(s => s.Id == sheet.Id);

        if (record == null)
            throw new InvalidOperationException($"Sheet with ID {sheet.Id} not found");

        var measureBytes = _serializationService.SerializeMeasurementData(sheet.Measures);

        record.Name = sheet.Name;
        record.Description = sheet.Description;
        record.Tempo = sheet.Tempo.Value;
        record.MeasureBytes = measureBytes;
        record.UpdatedAt = updatedAt;

        try
        {
            _context.Sheets.Update(record);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Sheets_UserId_Name") == true)
        {
            throw new InvalidOperationException("A sheet with that name already exists", ex);
        }
    }

    public async Task DeleteSheetAsync(Guid id, string? userId)
    {
        var query = _context.Sheets.AsQueryable();
        
        query = query.Where(s => s.UserId == userId);


        var record = await query.FirstOrDefaultAsync(s => s.Id == id);

        if (record == null)
            throw new InvalidOperationException($"Sheet with ID {id} not found");

        _context.Sheets.Remove(record);
        await _context.SaveChangesAsync();
    }
    // public async Task UpdateSheetUserIdAsync(Guid sheetId, string userId)
    // {
    //     var record = await _context.Sheets.FirstAsync(s => s.Id == sheetId);
    //     record.UserId = userId;
    //     await _context.SaveChangesAsync();
    // }

    public async Task CreateUserIfNotExistsAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            var userName = $"Local_{userId.Substring(0, 8)}"; // e.g. Local_72e6e134
            _context.Users.Add(new User { Id = userId, UserName = userName });
            await _context.SaveChangesAsync();
        }
    }
    public bool SheetExists(string sheetName, string? userId)
    {
        IQueryable<SheetRecord> query = _context.Sheets;
        return query.Any(s =>
            s.Name == sheetName && s.UserId == userId);
    }

    public static string GenerateCopyName(string originalName, HashSet<string?> existingNames)
    {
        var baseName = $"{originalName} - Copy";
        if (!existingNames.Contains(baseName))
            return baseName;
        var counter = 1;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName));

        return newName;
    }

    private Sheet DeserializeToSheet(SheetRecord record)
    {
        var measures = _serializationService.DeserializeMeasurementData(record.MeasureBytes);
        
        return new Sheet(
            new Bpm(record.Tempo),
            [..measures],
            record.Name,
            record.Description,
            record.Id,
            record.UpdatedAt)
        {
        };
    }
}
