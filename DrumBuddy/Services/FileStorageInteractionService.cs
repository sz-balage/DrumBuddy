using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Services;
using DrumBuddy.Models.Exceptions;
using DrumBuddy.ViewModels;

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
            await configurationService.SetAsync(LastJsonFolderKey, parentFolder);

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
            await configurationService.SetAsync(LastMidiFolderKey, parentFolder);

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
            await configurationService.SetAsync(LastMusicXmlFolderKey, parentFolder);

        MusicXmlExporter.ExportSheetToMusicXml(sheet, file.Path.AbsolutePath);

        return file.Name;
    }

    public async Task<(List<Sheet> sheets, List<SheetImportException> exceptions)>  OpenSheetsAsync(TopLevel topLevel)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return (new List<Sheet>(), new List<SheetImportException>());

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
        var sheetsAndExceptions = await ImportFiles(files);

        var parentFolder = Path.GetDirectoryName(files.FirstOrDefault()?.Path.LocalPath);
        if (parentFolder is not null)
            await configurationService.SetAsync(LastImportFolderKey, parentFolder);

        return sheetsAndExceptions;
    }

    public async Task<int> BatchExportSheetsAsync(
        TopLevel topLevel,
        IEnumerable<Sheet> sheets,
        SaveFormat format)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return 0;

        var folder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = $"Select export folder for {format} files...",
            AllowMultiple = false
        });

        var selectedFolder = folder.FirstOrDefault();
        if (selectedFolder is null)
            return 0;

        var basePath = selectedFolder.Path.LocalPath;
        Directory.CreateDirectory(basePath);

        var exportedCount = 0;

        var fileExtension = GetFileExtension(format);
        var relevantExistingFiles = Directory
            .EnumerateFiles(basePath)
            .Where(f => Path.GetExtension(f) == fileExtension)
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var sheet in sheets)
            try
            {
                var fileName = sheet.Name;

                if (relevantExistingFiles.Contains(fileName))
                    fileName = SheetService.GenerateCopyName(fileName, relevantExistingFiles);
                var fileNameWithExtension = $"{fileName}{fileExtension}";
                var filePath = Path.Combine(basePath, fileNameWithExtension);

                switch (format)
                {
                    case SaveFormat.Json:
                        var json = serializationService.SerializeSheet(sheet);
                        await File.WriteAllTextAsync(filePath, json);
                        break;

                    case SaveFormat.Midi:
                        midiService.ExportToMidi(sheet, filePath);
                        break;

                    case SaveFormat.MusicXml:
                        MusicXmlExporter.ExportSheetToMusicXml(sheet, filePath);
                        break;
                }

                exportedCount++;
            }
            catch
            {
            }

        return exportedCount;
    }

    private static string GetFileExtension(SaveFormat format)
    {
        return format switch
        {
            SaveFormat.Json => ".dbsheet",
            SaveFormat.Midi => ".midi",
            SaveFormat.MusicXml => ".xml",
            _ => ".dat"
        };
    }

    private async Task<(List<Sheet>, List<SheetImportException>)> ImportFiles(IReadOnlyList<IStorageFile> files)
    {
        var sheets = new List<Sheet>();
        var exceptions = new List<SheetImportException>();
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            Sheet? sheet = null;
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (extension is ".dbsheet")
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                try
                {
                    sheet = serializationService.DeserializeDbSheetFile(json, fileName);
                }
                catch (Exception e)
                {
                    exceptions.Add(new SheetImportException(e.Message, fileName));
                }
            }
            else if (extension is ".mid" or ".midi")
            {
                try
                {
                    sheet = midiService.ImportFromMidi(file.Path.AbsolutePath);
                }
                catch (Exception e)
                {
                    exceptions.Add(new SheetImportException(e.Message, fileName));
                }
            }
            else if (extension is ".musicxml" or ".xml")
            {
                try
                {
                    sheet = MusicXmlExporter.ImportMusicXmlToSheet(file.Path.AbsolutePath,
                        Path.GetFileNameWithoutExtension(file.Path.AbsolutePath));
                }
                catch (Exception e)
                {
                    exceptions.Add(new SheetImportException(e.Message, fileName));
                }
            }

            if (sheet is not null)
                sheets.Add(sheet);
        }

        return (sheets, exceptions);
    }
}