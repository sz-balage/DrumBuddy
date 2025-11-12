using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DrumBuddy.Api;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace DrumBuddy.ViewModels;

public partial class LibraryViewModel : ReactiveObject, ILibraryViewModel
{
    // TODO: add batch import/export
    private readonly FileStorageInteractionService _fileStorageInteractionService;

    private readonly MainWindow _mainWindow;
    private readonly MidiService _midiService;

    private readonly NotificationService _notificationService;
    private readonly PdfGenerator _pdfGenerator;
    private readonly IObservable<bool> _removeCanExecute;
    private readonly ReadOnlyObservableCollection<SheetViewModel> _sheets;
    private readonly SourceCache<SheetViewModel, string> _sheetSource = new(s => s.Name);
    private readonly SheetService _sheetService;
    private readonly ObservableAsPropertyHelper<SortOption> _sortOptionHelper;
    [Reactive] private string _filterText = string.Empty;
    [Reactive] private bool _isSortDescending;
    [Reactive] private bool _isLoadingSheets;
    [Reactive] private SheetViewModel _selectedSheet;
    [Reactive] private SortOption _selectedSortOption = SortOption.Name;
    private readonly UserService _userService;

    public LibraryViewModel(IScreen hostScreen, SheetService sheetService,
        PdfGenerator pdfGenerator,
        FileStorageInteractionService fileStorageInteractionService,
        MidiService midiService)
    {
        _userService = Locator.Current.GetRequiredService<UserService>();
        _mainWindow = Locator.Current.GetRequiredService<MainWindow>();
        _notificationService = new NotificationService(_mainWindow);
        _pdfGenerator = pdfGenerator;
        _fileStorageInteractionService = fileStorageInteractionService;
        _midiService = midiService;
        HostScreen = hostScreen;
        _sheetService = sheetService;
        var sortChanged = this.WhenAnyValue(vm => vm.SelectedSortOption, vm => vm.IsSortDescending)
            .Select(tuple =>
            {
                var (option, descending) = tuple;
                return option switch
                {
                    SortOption.Tempo => descending
                        ? SortExpressionComparer<SheetViewModel>.Descending(s => s.Tempo.Value)
                        : SortExpressionComparer<SheetViewModel>.Ascending(s => s.Tempo.Value),
                    SortOption.Length => descending
                        ? SortExpressionComparer<SheetViewModel>.Descending(s => s.Length)
                        : SortExpressionComparer<SheetViewModel>.Ascending(s => s.Length),
                    _ => descending
                        ? SortExpressionComparer<SheetViewModel>.Descending(s => s.Name)
                        : SortExpressionComparer<SheetViewModel>.Ascending(s => s.Name)
                };
            });
        var filter = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .DistinctUntilChanged()
            .Select(text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                    return _ => true;

                return new Func<SheetViewModel, bool>(sheet =>
                    (sheet.Name?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (sheet.Description?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
            });
        _sheetSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Filter(filter)
            .SortAndBind(out _sheets, sortChanged)
            .Subscribe();
        // _sheetSource.AddOrUpdate(new Sheet(new Bpm(100), ImmutableArray<Measure>.Empty, "New Sheet", "New Sheet"));
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null!);
        this.WhenNavigatedTo(() => LoadSheets()
            .ToObservable()
            .Subscribe());
    }
    public bool IsOnline => _userService.IsOnline;
    public IEnumerable<SortOption> SortOptions => Enum.GetValues<SortOption>();

    public ReadOnlyObservableCollection<SheetViewModel> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public async Task SaveSheet(Sheet sheet)
    {
        await _sheetService.CreateSheetAsync(sheet);
    }

    public async Task CompareSheets(Sheet baseSheet, Sheet comparedSheet)
    {
        _ = await ShowCompareDialog.Handle((baseSheet, comparedSheet));
    }

    public async Task BatchRemoveSheets(List<SheetViewModel> sheetsToRemove)
    {
        var confirmationVm = new ConfirmationViewModel
        {
            Message = "Are you sure you want to delete selected sheets?",
            ShowDiscard = true,
            ShowConfirm = false,
            DiscardText = "Delete",
            CancelText = "Cancel"
        };
        var confirmation = await ShowConfirmationDialog.Handle(confirmationVm);
        if (confirmation == Confirmation.Cancel)
            return;
        foreach (var sheet in sheetsToRemove)
        {
            await _sheetService.DeleteSheetAsync(sheet.Sheet);
            _sheetSource.Remove(sheet);
        }
    }

    public async void BatchExportSheets(List<SheetViewModel> selected, SaveFormat saveFormat)
    {
        if (selected.Count == 0)
            return;
        try
        {
            var count = await _fileStorageInteractionService.BatchExportSheetsAsync(
                _mainWindow,
                selected.Select(s => s.Sheet).ToList(),
                saveFormat);
            if (count > 0)
                _notificationService.ShowNotification(new Notification(
                    "Export complete.",
                    $"Successfully exported {count} sheet(s) to {saveFormat}.",
                    NotificationType.Success));
            else
                _notificationService.ShowNotification(new Notification(
                    "Export cancelled.",
                    "No sheets were exported.",
                    NotificationType.Warning));
        }
        catch (Exception ex)
        {
            _notificationService.ShowNotification(new Notification(
                "Export failed.",
                $"An error occurred while exporting sheets: {ex.Message}",
                NotificationType.Error));
        }
    }

    public Interaction<Sheet, Sheet> ShowRenameDialog { get; } = new();
    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();
    public Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; } = new();
    public Interaction<ConfirmationViewModel, Confirmation> ShowConfirmationDialog { get; } = new();


    public bool SheetExists(string sheetName)
    {
        return _sheetService.SheetExists(sheetName);
    }

    public async Task SaveSelectedSheetAs(SaveFormat format)
    {
        try
        {
            string? file;
            if (format == SaveFormat.Midi)
                file = await _fileStorageInteractionService.SaveSheetMidiAsync(_mainWindow, SelectedSheet.Sheet);
            else if (format == SaveFormat.Json)
                file = await _fileStorageInteractionService.SaveSheetJsonAsync(_mainWindow, SelectedSheet.Sheet);
            else
            {
                file = await _fileStorageInteractionService.SaveSheetMusicXmlAsync(_mainWindow, SelectedSheet.Sheet);
            }

            if (file is not null)
                _notificationService.ShowNotification(new Notification("Successful save.",
                    $"The sheet {SelectedSheet.Name} successfully saved to {format.ToString()} {file}.",
                    NotificationType.Success));
        }
        catch (Exception e)
        {
            _notificationService.ShowNotification(new Notification("Error saving sheet.",
                $"An error occurred while saving the sheet ({format.ToString()}): {e.Message}",
                NotificationType.Error));
        }
    }

    [ReactiveCommand]
    private void SortByAscending()
    {
        IsSortDescending = false;
    }

    [ReactiveCommand]
    private void SortByDescending()
    {
        IsSortDescending = true;
    }

    [ReactiveCommand]
    private async Task ImportSheet()
    {
        try
        {
            var sheetsAndExceptions = await _fileStorageInteractionService.OpenSheetsAsync(_mainWindow);
            foreach (var sheet in sheetsAndExceptions.sheets)
            {
                if (_sheetService.SheetExists(sheet.Name))
                {
                    var trimmedName = sheet.Name.Length > 15
                        ? sheet.Name[..15] + "..."
                        : sheet.Name;
                    var confirmationVm = new ConfirmationViewModel
                    {
                        Message = $"A sheet with the name {trimmedName} already exists. Do you want to overwrite it?",
                        ShowDiscard = false,
                        ShowConfirm = true,
                        ConfirmText = "Overwrite",
                        CancelText = "Cancel"
                    };
                    var confirmation = await ShowConfirmationDialog.Handle(confirmationVm);
                    if (confirmation == Confirmation.Cancel)
                        return;
                    if (confirmation == Confirmation.Confirm)
                        await _sheetService.DeleteSheetAsync(
                            _sheetSource.Items.First(s =>
                                s.Name.Equals(sheet.Name, StringComparison.OrdinalIgnoreCase)).Sheet);
                }
                sheet.IsSyncEnabled = false; //by default, new sheets are not synced
                await _sheetService.CreateSheetAsync(sheet);
                _sheetSource.AddOrUpdate(new SheetViewModel(sheet));
                _notificationService.ShowNotification(new Notification(
                    "Sheet imported.",
                    $"Successfully imported \"{sheet.Name}\".",
                    NotificationType.Success));
            }

            foreach (var exception in sheetsAndExceptions.exceptions)
                _notificationService.ShowNotification(new Notification(
                    "Import failed.",
                    $"An error occurred while importing {exception.SheetName}. " +
                    $"Please ensure that the sheet is in the correct format and was exported using the client.",
                    NotificationType.Error));
        }
        catch (Exception ex)
        {
            _notificationService.ShowNotification(new Notification(
                "Import failed.",
                $"An error occurred while importing the sheet: {ex.Message}",
                NotificationType.Error));
        }
    }

    [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
    private async Task RemoveSheet()
    {
        await _sheetService.DeleteSheetAsync(_selectedSheet.Sheet);
        _sheetSource.Remove(_selectedSheet);
    }

    [ReactiveCommand]
    private void NavigateToRecordingView()
    {
        var mainVm = HostScreen as MainViewModel;
        mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<RecordingViewModel>());
    }

    [ReactiveCommand]
    private void NavigateToManualView()
    {
        var mainVm = HostScreen as MainViewModel;
        mainVm!.NavigateFromCode(Locator.Current.GetRequiredService<ManualViewModel>());
    }

    [ReactiveCommand]
    private async Task RenameSheet()
    {
        var newSheet = await ShowRenameDialog.Handle(_selectedSheet.Sheet);
        if (newSheet != _selectedSheet.Sheet)
        {
            await _sheetService.UpdateSheetAsync(newSheet);
            _sheetSource.Remove(SelectedSheet);
            _sheetSource.AddOrUpdate(new SheetViewModel(newSheet));
            //SelectedSheet.RenameSheet(dialogResult);
        }
    }

    [ReactiveCommand]
    private async Task EditSheet()
    {
        var editResult = await ShowEditDialog.Handle(SelectedSheet.Sheet);
        if (editResult != null)
        {
            await _sheetService. UpdateSheetAsync(editResult);
            _sheetSource.AddOrUpdate(new SheetViewModel(editResult));
            _notificationService.ShowNotification(new Notification("Successful save.",
                $"The sheet {editResult.Name} successfully saved.",
                NotificationType.Success));
        }
    }

    [ReactiveCommand]
    private async Task ManuallyEditSheet()
    {
        var mainVm = HostScreen as MainViewModel;
        var manualVm = Locator.Current.GetRequiredService<ManualViewModel>();
        mainVm!.NavigateFromCode(manualVm);
        manualVm.ChooseSheet(SelectedSheet.Sheet);
    }

    [ReactiveCommand]
    private async Task DuplicateSheet()
    {
        var original = SelectedSheet;
        var allNames = _sheetSource.Items.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newName = SheetService.GenerateCopyName(original.Name, allNames);

        // Duplicate the object (assuming Sheet is immutable or cloneable)
        var duplicated = original.Sheet.RenameSheet(newName, original.Description);

        await _sheetService.CreateSheetAsync(duplicated);
        _sheetSource.AddOrUpdate(new SheetViewModel(duplicated));

        _notificationService.ShowNotification(new Notification("Sheet duplicated.",
            $"A copy named \"{newName}\" was created.",
            NotificationType.Success));
    }

    [ReactiveCommand]
    private async Task TurnOnSyncForSelectedSheet(SheetViewModel sheet)
    {
        sheet.IsSyncing = true;
        sheet.IsSyncEnabled = true;
        var previousLastSyncedAt = sheet.Sheet.LastSyncedAt;
        sheet.LastSyncedAt = DateTime.Now;
        var syncSuccessful = await _sheetService.SyncSheetToServer(sheet.Sheet);
        if (syncSuccessful)
        {
            _notificationService.ShowNotification(new Notification(
                "Sheet synced.",
                $"The sheet {sheet.Name} was successfully synced to the server.",
                NotificationType.Success));
        }
        else
        {
            sheet.Sheet.LastSyncedAt = previousLastSyncedAt; 
            sheet.IsSyncEnabled = false;
            _notificationService.ShowNotification(new Notification(
                "Sync failed.",
                $"An error occurred while syncing the sheet {sheet.Name} to the server. Sync has been disabled.",
                NotificationType.Error));
        }
        sheet.IsSyncing = false;
    }
    [ReactiveCommand]
    private async Task TurnOffSyncForSelectedSheet(SheetViewModel sheet)
    {     
        sheet.IsSyncing = true;
        sheet.IsSyncEnabled = false;
        var syncSuccessful = await _sheetService.UnSyncSheetToServer(sheet.Sheet);
        if (syncSuccessful)
        {
            _notificationService.ShowNotification(new Notification(
                "Sheet sync turned off.",
                $"The sheet {sheet.Name} was successfully removed from the server.",
                NotificationType.Success));
        }
        else
        {
            _notificationService.ShowNotification(new Notification(
                "Sync failed.",
                $"An error occurred while removing the sheet {sheet.Name} from the server.",
                NotificationType.Error));
        }
        sheet.IsSyncing = false;
    }

    private async Task LoadSheets()
    {
        IsLoadingSheets = true;
        try
        { 
            var sheets = await _sheetService.LoadSheetsAsync();
            _sheetSource.Clear();
            _sheetSource.AddOrUpdate(sheets.Select(s => new SheetViewModel(s)));
        } 
        finally   
        {
            IsLoadingSheets = false;
        }
    }
}

public enum SaveFormat
{
    Json,
    Midi,
    MusicXml
}

public interface ILibraryViewModel : IRoutableViewModel
{
    ReadOnlyObservableCollection<SheetViewModel> Sheets { get; }
    ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; }
    ReactiveCommand<Unit, Unit> RenameSheetCommand { get; }
    ReactiveCommand<Unit, Unit> EditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> ManuallyEditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToManualViewCommand { get; }
    SheetViewModel? SelectedSheet { get; set; }
    Interaction<Sheet, Sheet?> ShowEditDialog { get; }
    Interaction<Sheet, Sheet> ShowRenameDialog { get; }
    Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; }
    Interaction<ConfirmationViewModel, Confirmation> ShowConfirmationDialog { get; }
    ReactiveCommand<Unit, Unit> DuplicateSheetCommand { get; }
    bool SheetExists(string sheetName);
    Task SaveSheet(Sheet sheet);
    Task SaveSelectedSheetAs(SaveFormat format);
    Task CompareSheets(Sheet baseSheet, Sheet comparedSheet);
    Task BatchRemoveSheets(List<SheetViewModel> selected);
    void BatchExportSheets(List<SheetViewModel> selected, SaveFormat saveFormat);
    ReactiveCommand<SheetViewModel, Unit> TurnOnSyncForSelectedSheetCommand { get; }
    ReactiveCommand<SheetViewModel, Unit> TurnOffSyncForSelectedSheetCommand { get; }
}