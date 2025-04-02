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
        // var normalizedName = NormalizeFileName(sheet.Name);
        // var filePath = GetFullPath(normalizedName);
        var serialized = _serializationService.SerializeMeasurementData(sheet.Measures);
        await SheetDbCommands.InsertSheetAsync(_connectionString, sheet.Name, sheet.Tempo.Value, serialized, sheet.Description);
        // await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
    }

    public async Task RemoveSheetAsync(Sheet sheet)
    {
        var fileName = NormalizeFileName(sheet.Name);
        var filePath = GetFullPath(fileName);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Sheet file not found: {fileName}");
        await Task.Run(() => File.Delete(filePath));    
    }
    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync()
    {
        // var filePaths = Directory.EnumerateFiles(_saveDirectory,$"*{FileExtension}").ToList();
        var dbRecords = await SheetDbQueries.SelectAllSheetsAsync(_connectionString);
        var sheets = dbRecords.Select(r =>
            new Sheet(new Bpm(r.Tempo), [.._serializationService.DeserializeMeasurementData(r.MeasuresData)], r.Name,
                r.Description));
        
        return [..sheets];
        // foreach (var filePath in filePaths)
        // {
        //     var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        //     var sheet = _serializationService.DeserializeSheet(json); //TODO: cannot deserialize BPM property
        //     arrayBuilder.Add(sheet);
        // }
        // return arrayBuilder.MoveToImmutable();
    }

    public async Task<IEnumerable<string>> GetSavedSheetNames()
    {
        return (await Task.Run(() =>
            Directory.GetFiles(_saveDirectory, $"*{FileExtension}")
                .Select(p => DenormalizeFileName(Path.GetFileNameWithoutExtension(p)))))!;
    }

    /// <summary>
    /// Renames file from old sheet name to new sheet name.
    /// </summary>
    /// <param name="oldSheetName"></param>
    /// <param name="newSheet"></param>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public async Task RenameFileAsync(string oldSheetName, Sheet newSheet)
    {
        var oldFileName = NormalizeFileName(oldSheetName);
        var oldPath = GetFullPath(oldFileName);
        File.Delete(oldPath);
        await SaveSheetAsync(newSheet);
    }

    public bool SheetExists(string sheetName)
    {
        var filePath = GetFullPath(NormalizeFileName(sheetName));
        return File.Exists(filePath);
    }

    private string NormalizeFileName(string fileName)
    {
        var spacesReplaced = fileName.Replace(' ', '_');

        // Replace invalid filename characters with underscores
        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var invalidCharsPattern = $"[{Regex.Escape(invalidChars)}]";
        var normalized = Regex.Replace(spacesReplaced, invalidCharsPattern, "_");
        return normalized;
    }

    private string DenormalizeFileName(string fileName) => fileName.Replace('_', ' ');

    private string GetFullPath(string fileName) => Path.Combine(_saveDirectory, fileName + FileExtension);
}