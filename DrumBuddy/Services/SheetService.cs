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
    private readonly IApiClient _apiClient;
    private readonly ISheetRepository _repository;
    private readonly IUserService _userService;

    public SheetService(
        ISheetRepository repository,
        IUserService userService,
        IApiClient apiClient)
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

        return await _repository.LoadSheetsAsync(_userService.UserId);
    }

    /// <summary>
    ///     Get a specific sheet by ID
    /// </summary>
    public async Task<Sheet?> GetSheetByIdAsync(Guid id)
    {
        return await _repository.GetSheetByIdAsync(id, _userService.UserId);
    }

    /// <summary>
    ///     Create a new sheet locally and sync to server if online
    /// </summary>
    public async Task CreateSheetAsync(Sheet sheet)
    {
        await _repository.SaveSheetAsync(sheet, _userService.UserId); 
    }

    public async Task<bool> SyncSheetToServer(Sheet sheet)
    {
        var updatedAt = DateTime.UtcNow;
        if (sheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.CreateSheetAsync(sheet, updatedAt);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        return false;
    }

    public async Task<bool> UnSyncSheetToServer(Sheet sheet)
    {
        await _repository.UpdateSheetAsync(sheet, DateTime.UtcNow, _userService.UserId);
        try
        {
            await _apiClient.DeleteSheetAsync(sheet.Id);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    ///     Update an existing sheet locally and sync to server if online
    /// </summary>
    public async Task UpdateSheetAsync(Sheet sheet)
    {
        var updatedAt = DateTime.UtcNow;
        await _repository.UpdateSheetAsync(sheet, updatedAt, _userService.UserId); //local update userid null

        if (sheet.IsSyncEnabled && _userService.IsOnline)
            try
            {
                await _apiClient.UpdateSheetAsync(sheet.Id, sheet, updatedAt);
                sheet.LastSyncedAt = DateTime.Now;
            }
            catch
            {
                sheet.IsSyncEnabled = false;
            }
    }

    /// <summary>
    ///     Delete a sheet locally and from server if online
    /// </summary>
    public async Task DeleteSheetAsync(Sheet sheet)
    {
        await _repository.DeleteSheetAsync(sheet.Id, _userService.UserId);

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
        return _repository.SheetExists(sheetName, _userService.UserId);
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
        var localSheets = (await _repository.LoadSheetsAsync(_userService.UserId)).ToList();
        var remoteSummaries = await _apiClient.GetSheetSummariesAsync();

        var localMap = localSheets.ToDictionary(s => s.Id);
        var localByName = localSheets.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var remoteSummary in remoteSummaries)
        {
            if (localByName.TryGetValue(remoteSummary.Name, out var localWithSameName)
                && localWithSameName.Id != remoteSummary.Id)
            {
                //name conflict, resolve based on UpdatedAt
                if (localWithSameName.UpdatedAt >= remoteSummary.UpdatedAt)
                {
                    // local sheet is newer, delete remote version
                    await _apiClient.DeleteSheetAsync(remoteSummary.Id);
                    continue;
                }

                // server sheet is newer, delete local version & download server one
                await _repository.DeleteSheetAsync(localWithSameName.Id, _userService.UserId);
                var remoteFull = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.SaveSheetAsync(remoteFull, _userService.UserId);
                localSheets.Remove(localWithSameName);
                localSheets.Add(remoteFull.Sync());
                continue;
            }

            if (!localMap.TryGetValue(remoteSummary.Id, out var local))
            {
                var remote = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.SaveSheetAsync(remote, _userService.UserId);
                localSheets.Add(remote.Sync());
                continue;
            }

            if (remoteSummary.UpdatedAt > local.UpdatedAt)
            {
                var remote = await _apiClient.GetSheetAsync(remoteSummary.Id);
                await _repository.UpdateSheetAsync(remote, remote.UpdatedAt, _userService.UserId);
                localSheets.Remove(local);
                localSheets.Add(remote.Sync());
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