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
    ///     Load all sheets from local database
    ///     If online, also sync with server
    /// </summary>
    public async Task<ImmutableArray<Sheet>> LoadSheetsAsync()
    {
        if (_userService.IsOnline)
        {
            var syncedSheets = await SyncWithServerAsync();
            return syncedSheets;
        }

        return await _repository.LoadSheetsAsync();
    }

    /// <summary>
    ///     Get a specific sheet by ID
    /// </summary>
    public async Task<Sheet?> GetSheetByIdAsync(Guid id)
    {
        return await _repository.GetSheetByIdAsync(id);
    }

    /// <summary>
    ///     Create a new sheet locally and sync to server if online
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
        var updatedAt = DateTime.UtcNow;
        await _repository.UpdateSheetAsync(sheet, updatedAt);
        if (sheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.CreateSheetAsync(sheet, updatedAt);
                return true;
            }
            catch (Exception ex)
            {
                ;
                return false;
            }

        return false;
    }

    public async Task<bool> UnSyncSheetToServer(Sheet sheet)
    {
        await _repository.UpdateSheetAsync(sheet, DateTime.UtcNow);
        try
        {
            await _apiClient.DeleteSheetAsync(sheet.Id);
            return true;
        }
        catch (Exception ex)
        {
            ;
            return false;
        }
    }

    /// <summary>
    ///     Update an existing sheet locally and sync to server if online
    /// </summary>
    public async Task UpdateSheetAsync(Sheet sheet)
    {
        var updatedAt = DateTime.UtcNow;
        await _repository.UpdateSheetAsync(sheet, updatedAt); //local update userid null

        // Sync to server if logged in
        if (sheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.UpdateSheetAsync(sheet.Id, sheet, updatedAt);
                sheet.LastSyncedAt = DateTime.Now;
            }
            catch
            {
                // Offline or sync failed - mark for later sync
                sheet.IsSyncEnabled = false;
            }
    }

    /// <summary>
    ///     Delete a sheet locally and from server if online
    /// </summary>
    public async Task DeleteSheetAsync(Sheet sheet)
    {
        // Delete from local database
        await _repository.DeleteSheetAsync(sheet.Id);

        // Delete from server if logged in
        if (sheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.DeleteSheetAsync(sheet.Id);
            }
            catch
            {
                //offline
            }
    }

    /// <summary>
    ///     Check if a sheet name exists locally
    /// </summary>
    public bool SheetExists(string sheetName)
    {
        return _repository.SheetExists(sheetName);
    }

    /// <summary>
    ///     Rename a sheet locally and sync to server if online
    /// </summary>
    public async Task RenameSheetAsync(string oldName, Sheet newSheet)
    {
        var updatedAt = DateTime.UtcNow;
        await _repository.RenameSheetAsync(oldName, newSheet, updatedAt);

        if (newSheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.UpdateSheetAsync(newSheet.Id, newSheet, updatedAt);
                newSheet.IsSyncEnabled = true;
                newSheet.LastSyncedAt = DateTime.Now;
            }
            catch
            {
                newSheet.IsSyncEnabled = false;
            }
    }

    /// <summary>
    ///     Generate a copy name for duplicating sheets
    /// </summary>
    public static string GenerateCopyName(string originalName, HashSet<string?> existingNames)
    {
        return SheetRepository.GenerateCopyName(originalName, existingNames);
    }

    /// <returns>Sheets to store locally, synced with server versions</returns>
    private async Task<ImmutableArray<Sheet>> SyncWithServerAsync()
    {
        var localSheets = (await _repository.LoadSheetsAsync()).ToList();
        var remoteSummaries = await _apiClient.GetSheetSummariesAsync();

        var localMap = localSheets.ToDictionary(s => s.Id);
        var localByName = localSheets.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var remoteSummary in remoteSummaries)
        {
            if (localByName.TryGetValue(remoteSummary.Name, out var localWithSameName)
                && localWithSameName.Id != remoteSummary.Id)
            {
                // name conflict — resolve based on UpdatedAt
                if (localWithSameName.UpdatedAt >= remoteSummary.UpdatedAt)
                {
                    // local sheet is newer → delete remote version
                    await _apiClient.DeleteSheetAsync(remoteSummary.Id);
                    continue;
                }

                // server sheet is newer → delete local version & download server one
                await _repository.DeleteSheetAsync(localWithSameName.Id);
                var remoteFull = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.SaveSheetAsync(remoteFull);
                localSheets.Remove(localWithSameName);
                localSheets.Add(remoteFull.Sync());
                continue;
            }

            if (!localMap.TryGetValue(remoteSummary.Id, out var local))
            {
                var remote = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.SaveSheetAsync(remote);
                localSheets.Add(remote.Sync());
                continue;
            }

            if (remoteSummary.UpdatedAt > local.UpdatedAt)
            {
                var remote = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.UpdateSheetAsync(remote, remote.UpdatedAt);
            }
            else if (local.UpdatedAt > remoteSummary.UpdatedAt)
            {
                await _apiClient.UpdateSheetAsync(local.Id, local, local.UpdatedAt);
            }

            local.Sync();
        }

        return [..localSheets];
    }
}