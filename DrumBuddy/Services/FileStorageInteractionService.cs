using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Services;

namespace DrumBuddy.Services;

public class FileStorageInteractionService(
    SerializationService serializationService,
    MidiService midiService,
    ConfigurationService configurationService)
{
    private const string LastJsonFolderKey = "LastUsedJsonFolder";
    private const string LastMidiFolderKey = "LastUsedMidiFolder";
    private const string LastMusicXmlFolderKey = "LastUsedMusicXmlFolder";
    private const string LastImportFolderKey = "LastUsedImportFolder";

    public async Task<string?> SaveSheetJsonAsync(TopLevel topLevel, Sheet sheet)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;

        var lastFolderPath = configurationService.Get<string>(LastJsonFolderKey);
        var fallbackPath = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "sheets");

        var basePath = !string.IsNullOrWhiteSpace(lastFolderPath) && Directory.Exists(lastFolderPath)
            ? lastFolderPath
            : fallbackPath;
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);
        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(basePath));
        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Save Sheet As...",
            SuggestedStartLocation = suggestedFolder,
            SuggestedFileName = sheet.Name,
            FileTypeChoices = new List<FilePickerFileType>
            {
                new("DrumBuddy Sheet File (*.dbsheet)") { Patterns = new[] { "*.dbsheet" } }
            }
        };

        var file = await storageProvider.SaveFilePickerAsync(filePickerOptions);
        if (file is null)
            return null;

        var data = serializationService.SerializeSheet(sheet);
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(data);

        var parentFolder = Path.GetDirectoryName(file.Path.LocalPath);
        if (parentFolder is not null)
            configurationService.Set(LastJsonFolderKey, parentFolder);

        return file.Name;
    }

    public async Task<string?> SaveSheetMidiAsync(TopLevel topLevel, Sheet sheet)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;

        var lastFolderPath = configurationService.Get<string>(LastMidiFolderKey);
        var fallbackPath = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "midi");

        var basePath = !string.IsNullOrWhiteSpace(lastFolderPath) && Directory.Exists(lastFolderPath)
            ? lastFolderPath
            : fallbackPath;

        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(basePath));

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Export as MIDI...",
            SuggestedStartLocation = suggestedFolder,
            SuggestedFileName = sheet.Name,
            FileTypeChoices = new List<FilePickerFileType>
            {
                new("MIDI File (*.midi)") { Patterns = new[] { "*.midi" } }
            }
        };

        var file = await storageProvider.SaveFilePickerAsync(filePickerOptions);
        if (file is null)
            return null;

        var parentFolder = Path.GetDirectoryName(file.Path.LocalPath);
        if (parentFolder is not null)
            configurationService.Set(LastMidiFolderKey, parentFolder);

        midiService.ExportToMidi(sheet, file.Path.AbsolutePath);

        return file.Name;
    }

    public async Task<string?> SaveSheetMusicXmlAsync(TopLevel topLevel, Sheet sheet)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;

        var lastFolderPath = configurationService.Get<string>(LastMusicXmlFolderKey);
        var fallbackPath = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "musicxml");

        var basePath = !string.IsNullOrWhiteSpace(lastFolderPath) && Directory.Exists(lastFolderPath)
            ? lastFolderPath
            : fallbackPath;

        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(basePath));

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Export as MusicXML...",
            SuggestedStartLocation = suggestedFolder,
            SuggestedFileName = sheet.Name,
            FileTypeChoices = new List<FilePickerFileType>
            {
                new("MusicXML File (*.musicxml, *.xml)") { Patterns = new[] { "*.xml", "*.musicxml" } },
                new("All Files") { Patterns = new[] { "*" } }
            }
        };

        var file = await storageProvider.SaveFilePickerAsync(filePickerOptions);
        if (file is null)
            return null;

        var parentFolder = Path.GetDirectoryName(file.Path.LocalPath);
        if (parentFolder is not null)
            configurationService.Set(LastMusicXmlFolderKey, parentFolder);

        MusicXmlExporter.ExportSheetToMusicXml(sheet, file.Path.AbsolutePath);

        return file.Name;
    }

    public async Task<List<Sheet>> OpenSheetAsync(TopLevel topLevel)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return new List<Sheet>();

        var lastFolderPath = configurationService.Get<string>(LastImportFolderKey);
        var fallbackPath = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "sheets");

        var basePath = !string.IsNullOrWhiteSpace(lastFolderPath) && Directory.Exists(lastFolderPath)
            ? lastFolderPath
            : fallbackPath;

        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(basePath));

        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Open Sheet or MIDI File...",
            AllowMultiple = true,
            SuggestedStartLocation = suggestedFolder,
            FileTypeFilter =
            [
                new FilePickerFileType("DrumBuddy compatible file (*.dbsheet,*.midi,*.xml,*.musicxml)")
                    { Patterns = new[] { "*.dbsheet", "*.midi", "*.xml", "*.musicxml" } }
            ]
        };

        var files = await storageProvider.OpenFilePickerAsync(filePickerOptions);
        var sheets = await ImportFiles(files);

        var parentFolder = Path.GetDirectoryName(files.FirstOrDefault()?.Path.LocalPath);
        if (parentFolder is not null)
            configurationService.Set(LastImportFolderKey, parentFolder);

        return sheets;
    }

    private async Task<List<Sheet>> ImportFiles(IReadOnlyList<IStorageFile> files)
    {
        var sheets = new List<Sheet>();
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            Sheet? sheet = null;

            if (extension is ".dbsheet")
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var fileName = Path.GetFileNameWithoutExtension(file.Name);
                sheet = serializationService.DeserializeSheet(json, fileName);
            }
            else if (extension is ".mid" or ".midi")
            {
                sheet = midiService.ImportFromMidi(file.Path.AbsolutePath);
            }
            else if (extension is ".musicxml" or ".xml")
            {
                sheet = MusicXmlExporter.ImportMusicXmlToSheet(file.Path.AbsolutePath,
                    Path.GetFileNameWithoutExtension(file.Path.AbsolutePath));
            }

            if (sheet is not null)
                sheets.Add(sheet);
        }

        return sheets;
    }
}