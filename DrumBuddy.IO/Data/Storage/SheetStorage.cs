using System.Collections.Immutable;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data.Commands;

namespace DrumBuddy.IO.Data.Storage;

public class SheetStorage
{
    private readonly string _connectionString;
    private readonly SerializationService _serializationService;

    public SheetStorage(SerializationService serializationService, string connectionString)
    {
        _serializationService = serializationService;

        _connectionString = connectionString;
    }

    public async Task SaveSheetAsync(Sheet sheet)
    {
        var serialized = _serializationService.SerializeMeasurementData(sheet.Measures);
        await SheetDbCommands.InsertSheetAsync(_connectionString, sheet.Name, sheet.Tempo.Value, serialized,
            sheet.Description);
    }

    public async Task RemoveSheetAsync(Sheet sheet)
    {
        await SheetDbCommands.DeleteSheetAsync(_connectionString, sheet.Name);
    }

    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync()
    {
        var dbRecords = await SheetDbQueries.SelectAllSheetsAsync(_connectionString);
        var sheets = dbRecords.Select(r =>
            new Sheet(new Bpm((int)r.Tempo),
                [.._serializationService.DeserializeMeasurementData(r.MeasuresData)],
                r.Name,
                r.Description));
        return [..sheets];
    }

    public async Task RenameSheetAsync(string oldSheetName, Sheet newSheet)
    {
        var serialized = _serializationService.SerializeMeasurementData(newSheet.Measures);
        await SheetDbCommands.UpdateSheetAsync(_connectionString, oldSheetName, newSheet.Tempo.Value, serialized,
            newSheet.Name, newSheet.Description);
    }

    public bool SheetExists(string sheetName)
    {
        return SheetDbQueries.SheetExists(_connectionString, sheetName);
    }

    public Task UpdateSheetAsync(Sheet sheet)
    {
        var serialized = _serializationService.SerializeMeasurementData(sheet.Measures);
        return SheetDbCommands.UpdateSheetAsync(_connectionString, sheet.Name, sheet.Tempo.Value, serialized,
            sheet.Name, sheet.Description);
    }

    public static string GenerateCopyName(string originalName, HashSet<string> existingNames)
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
}