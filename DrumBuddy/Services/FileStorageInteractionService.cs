using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;

namespace DrumBuddy.Services;

public class FileStorageInteractionService(SerializationService serializationService)
{
    public async Task<string?> SaveSheetAsAsync(TopLevel topLevel, Sheet sheet)
    {
        var storageProvider = topLevel?.StorageProvider;
        if (storageProvider is null)
            return null;
        var suggestedPath = Path.Combine(FilePathProvider.GetPathForFileStorage(), "sheets");
        if (!Directory.Exists(suggestedPath))
            Directory.CreateDirectory(suggestedPath);
        var suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(new Uri(suggestedPath));
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

        var data = await serializationService.SerializeSheet(sheet);
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(data);
        return file.Name;
    }
}