using System.Collections.Immutable;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
using DrumBuddy.Core.Abstractions;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Abstractions;
using DrumBuddy.IO.Data;

namespace DrumBuddy.IO.Services;

public class SheetStorage : ISheetStorage //TODO: look at sqlite for storing sheets
{
    private readonly ISerializationService _serializationService;
    private readonly string _saveDirectory;
    private const string FileExtension = ".dby";
    private readonly string _connectionString;
    public SheetStorage(ISerializationService serializationService, string connectionString)
    {
        _serializationService = serializationService;

        _connectionString = connectionString;
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "SavedFiles");

        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }
    public async Task SaveSheetAsync(Sheet sheet)
    {
        var serialized = _serializationService.SerializeMeasurementData(sheet.Measures);
        await SheetDbCommands.InsertSheetAsync(_connectionString, sheet.Name, sheet.Tempo.Value, serialized, sheet.Description);
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
        await SheetDbCommands.UpdateSheetAsync(_connectionString, oldSheetName, newSheet.Tempo.Value, serialized, newSheet.Name, newSheet.Description);
    }

    public bool SheetExists(string sheetName)
    {
        return SheetDbQueries.SheetExists(_connectionString, sheetName);
    }

    private string DenormalizeFileName(string fileName) => fileName.Replace('_', ' ');

    private string GetFullPath(string fileName) => Path.Combine(_saveDirectory, fileName + FileExtension);
}