using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Services;

namespace DrumBuddy.Services;

public class FileStorageInteractionService(
    SerializationService serializationService,
    ConfigurationService configurationService)
{
    private const string LastFolderKey = "LastUsedSheetFolder";

    public async Task<string?> SaveSheetAsAsync(TopLevel topLevel, Sheet sheet)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;

        var lastFolderPath = configurationService.Get<string>(LastFolderKey);
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
                new("DrumBuddy Sheet File (*.dbsheet)") { Patterns = new[] { "*.dbsheet" } },
                new("All Files") { Patterns = new[] { "*" } }
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
            configurationService.Set(LastFolderKey, parentFolder);

        return file.Name;
    }

    public async Task<Sheet?> OpenSheetAsync(TopLevel topLevel)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;

        var lastFolderPath = configurationService.Get<string>(LastFolderKey);
        var fallbackPath = Path.Combine(FilePathProvider.GetPathForSavedFiles(), "sheets");

        var basePath = !string.IsNullOrWhiteSpace(lastFolderPath) && Directory.Exists(lastFolderPath)
            ? lastFolderPath
            : fallbackPath;

        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(basePath));

        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "Import Sheet...",
            AllowMultiple = false,
            SuggestedStartLocation = suggestedFolder,
            FileTypeFilter =
            [
                new FilePickerFileType("DrumBuddy Sheet File (*.dbsheet)") { Patterns = new[] { "*.dbsheet" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            ]
        };

        var files = await storageProvider.OpenFilePickerAsync(filePickerOptions);
        var file = files.Count > 0 ? files[0] : null;
        if (file is null)
            return null;

        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var data = await reader.ReadToEndAsync();

        var fileName = Path.GetFileNameWithoutExtension(file.Name);
        var sheet = serializationService.DeserializeSheet(data, fileName);

        var parentFolder = Path.GetDirectoryName(file.Path.LocalPath);
        if (parentFolder is not null)
            configurationService.Set(LastFolderKey, parentFolder);

        return sheet;
    }
}