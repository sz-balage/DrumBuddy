using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DrumBuddy.Api;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Data;

namespace DrumBuddy.Services;

public class SheetService
{
    //sheets are always saved with userid set to null to the local, sqlite db
    //on the server side, sheets are saved with the actual userid
    //when syncing, server will only return sheets for the logged in user, and if sheet is synced,
    //all requests will contain the userid
    private readonly SheetRepository _repository;
    private readonly UserService _userService;
    private readonly ApiClient _apiClient;

    public SheetService( 
        SheetRepository repository,
        UserService userService,
        ApiClient apiClient)
    {
        _repository = repository;
        _userService = userService;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Load all sheets from local database
    /// If online, also sync with server
    /// </summary>
    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync()
    {
        var sheets = 
            await _repository.LoadSheetsAsync();
        if (_userService.IsOnline)
        {
            try
            {
                // var localSheets = _repository.LoadSheetsAsync(null);
                // // if a user is logged in, we still have to preserve the uniqueness of sheet names
                // await SyncWithServerAsync(sheets);
            }
            catch
            {
                //offline or sync failed -> return local sheets
            }
        }

        return sheets;
    }

    /// <summary>
    /// Get a specific sheet by ID
    /// </summary>
    public async Task<Sheet?> GetSheetByIdAsync(Guid id)
    {
        return await _repository.GetSheetByIdAsync(id);
    }

    /// <summary>
    /// Create a new sheet locally and sync to server if online
    /// </summary>
    public async Task CreateSheetAsync(Sheet sheet)
    {
        await _repository.SaveSheetAsync(sheet); //local save, userid null
        // if (sheet.IsSyncEnabled && _userService.IsOnline)
        // {
        //     try
        //     {
        //         await _apiClient.CreateSheetAsync(sheet);
        //         sheet.IsSyncEnabled = true;
        //     }
        //     catch
        //     {
        //         sheet.IsSyncEnabled = false;
        //     }
        // }
    }
    public async Task<bool> SyncSheetToServer(Sheet sheet)
    {
        await _repository.UpdateSheetAsync(sheet);
        if (sheet.IsSyncEnabled && _userService.IsOnline)
        {
            try
            {
                await _apiClient.CreateSheetAsync(sheet);
                return true;
            }
            catch(Exception ex)
            {
                ;
                return false;
            }
        }
        return false;
    }
    public async Task<bool> UnSyncSheetToServer(Sheet sheet)
    {
        await _repository.UpdateSheetAsync(sheet);
        try
        {
            await _apiClient.DeleteSheetAsync(sheet.Id);
            return true;
        }
        catch(Exception ex)
        {
            ;
            return false;
        }
    }

    /// <summary>
    /// Update an existing sheet locally and sync to server if online
    /// </summary>
    public async Task UpdateSheetAsync(Sheet sheet)
    {
        // Update local database
        await _repository.UpdateSheetAsync(sheet); //local update userid null

        // Sync to server if logged in
        if (sheet.IsSyncEnabled && _userService.IsOnline)
        {
            try
            {
                await _apiClient.UpdateSheetAsync(sheet.Id, sheet);
                sheet.IsSyncEnabled = true;
                sheet.LastSyncedAt = DateTime.UtcNow;
            }
            catch
            {
                // Offline or sync failed - mark for later sync
                sheet.IsSyncEnabled = false;
            }
        }
    }

    /// <summary>
    /// Delete a sheet locally and from server if online
    /// </summary>
    public async Task DeleteSheetAsync(Sheet sheet)
    {
        // Delete from local database
        await _repository.DeleteSheetAsync(sheet.Id);

        // Delete from server if logged in
        if (sheet.IsSyncEnabled && _userService.IsOnline)
        {
            try
            {
                await _apiClient.DeleteSheetAsync(sheet.Id);
            }
            catch
            {
                //offline
            }
        }
    }

    /// <summary>
    /// Check if a sheet name exists locally
    /// </summary>
    public bool SheetExists(string sheetName)
    {
        return _repository.SheetExists(sheetName);
    }

    /// <summary>
    /// Rename a sheet locally and sync to server if online
    /// </summary>
    public async Task RenameSheetAsync(string oldName, Sheet newSheet)
    {
        await _repository.RenameSheetAsync(oldName, newSheet);

        if (_userService.IsTokenValid())
        {
            try
            {
                await _apiClient.UpdateSheetAsync(newSheet.Id, newSheet);
                newSheet.IsSyncEnabled = true;
                newSheet.LastSyncedAt = DateTime.UtcNow;
            }
            catch
            {
                newSheet.IsSyncEnabled = false;
            }
        }
    }

    /// <summary>
    /// Generate a copy name for duplicating sheets
    /// </summary>
    public static string GenerateCopyName(string originalName, HashSet<string?> existingNames)
    {
        return SheetRepository.GenerateCopyName(originalName, existingNames);
    }

    /// <summary>
    /// Sync local sheets with server
    /// Merges changes and resolves conflicts
    /// </summary>
    private async Task SyncWithServerAsync(ImmutableArray<Sheet> localSheets)
    {
        var serverSheets = await _apiClient.GetSheetsAsync();
        var localDict = localSheets.ToDictionary(s => s.Id);

        foreach (var serverSheet in serverSheets)
        {
            if (!localDict.ContainsKey(serverSheet.Id))
            {
                await _repository.SaveSheetAsync(serverSheet);
            }
            else
            {
                var localSheet = localDict[serverSheet.Id];
                
                if (serverSheet.LastSyncedAt > localSheet.LastSyncedAt)
                {
                    await _repository.UpdateSheetAsync(serverSheet);
                }
            }
        }

        var serverIds = serverSheets.Select(s => s.Id).ToHashSet();
        foreach (var localId in localDict.Keys.Where(id => !serverIds.Contains(id)))
        {
            var localSheet = localDict[localId];
            
            if (localSheet.LastSyncedAt.HasValue)
            {
                await _repository.DeleteSheetAsync(localId);
            }
        }
    }
}
