using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Data.Storage;
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
    private readonly FileStorageInteractionService _fileStorageInteractionService;

    private readonly MainWindow _mainWindow;
    private readonly MidiService _midiService;

    private readonly NotificationService _notificationService;
    private readonly PdfGenerator _pdfGenerator;
    private readonly IObservable<bool> _removeCanExecute;
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly SheetStorage _sheetStorage;
    private readonly ObservableAsPropertyHelper<SortOption> _sortOptionHelper;
    [Reactive] private string _filterText = string.Empty;
    [Reactive] private bool _isSortDescending;
    [Reactive] private Sheet _selectedSheet;
    [Reactive] private SortOption _selectedSortOption = SortOption.Name;

    public LibraryViewModel(IScreen hostScreen, SheetStorage sheetStorage,
        PdfGenerator pdfGenerator,
        FileStorageInteractionService fileStorageInteractionService,
        MidiService midiService)
    {
        _mainWindow = Locator.Current.GetRequiredService<MainWindow>();
        _notificationService = new NotificationService(_mainWindow);
        _pdfGenerator = pdfGenerator;
        _fileStorageInteractionService = fileStorageInteractionService;
        _midiService = midiService;
        HostScreen = hostScreen;
        _sheetStorage = sheetStorage;
        var sortChanged = this.WhenAnyValue(vm => vm.SelectedSortOption, vm => vm.IsSortDescending)
            .Select(tuple =>
            {
                var (option, descending) = tuple;
                return option switch
                {
                    SortOption.Tempo => descending
                        ? SortExpressionComparer<Sheet>.Descending(s => s.Tempo.Value)
                        : SortExpressionComparer<Sheet>.Ascending(s => s.Tempo.Value),
                    SortOption.Length => descending
                        ? SortExpressionComparer<Sheet>.Descending(s => s.Length)
                        : SortExpressionComparer<Sheet>.Ascending(s => s.Length),
                    _ => descending
                        ? SortExpressionComparer<Sheet>.Descending(s => s.Name)
                        : SortExpressionComparer<Sheet>.Ascending(s => s.Name)
                };
            });
        var filter = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .DistinctUntilChanged()
            .Select(text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                    return _ => true;

                return new Func<Sheet, bool>(sheet =>
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

    public IEnumerable<SortOption> SortOptions => Enum.GetValues<SortOption>();

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public async Task SaveSheet(Sheet sheet)
    {
        await _sheetStorage.SaveSheetAsync(sheet);
    }

    public async Task CompareSheets(Sheet baseSheet, Sheet comparedSheet)
    {
        _ = await ShowCompareDialog.Handle((baseSheet, comparedSheet));
    }

    public async Task BatchRemoveSheets(List<Sheet> sheetsToRemove)
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
            await _sheetStorage.RemoveSheetAsync(sheet);
            _sheetSource.Remove(sheet);
        }
    }


    public Interaction<Sheet, Sheet> ShowRenameDialog { get; } = new();
    public Interaction<Sheet, Sheet?> ShowEditDialog { get; } = new();
    public Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; } = new();
    public Interaction<ConfirmationViewModel, Confirmation> ShowConfirmationDialog { get; } = new();


    public bool SheetExists(string sheetName)
    {
        return _sheetStorage.SheetExists(sheetName);
    }

    public async Task SaveSelectedSheetAs(SaveFormat format)
    {
        try
        {
            string? file;
            if (format == SaveFormat.Midi)
                file = await _fileStorageInteractionService.SaveSheetMidiAsync(_mainWindow, SelectedSheet);
            else if (format == SaveFormat.Json)
                file = await _fileStorageInteractionService.SaveSheetJsonAsync(_mainWindow, SelectedSheet);
            else
            {
                file = await _fileStorageInteractionService.SaveSheetMusicXmlAsync(_mainWindow, SelectedSheet);
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
            var sheet = await _fileStorageInteractionService.OpenSheetAsync(_mainWindow);
            if (sheet is null)
                return;

            if (_sheetStorage.SheetExists(sheet.Name))
            {
                var confirmationVm = new ConfirmationViewModel
                {
                    Message = "A sheet with this name already exists. Do you want to overwrite it?",
                    ShowDiscard = false,
                    ShowConfirm = true,
                    ConfirmText = "Overwrite",
                    CancelText = "Cancel"
                };
                var confirmation = await ShowConfirmationDialog.Handle(confirmationVm);
                if (confirmation == Confirmation.Cancel)
                    return;
                if (confirmation == Confirmation.Confirm)
                    await _sheetStorage.RemoveSheetAsync(
                        _sheetSource.Items.First(s => s.Name.Equals(sheet.Name, StringComparison.OrdinalIgnoreCase)));
            }

            await _sheetStorage.SaveSheetAsync(sheet);
            _sheetSource.AddOrUpdate(sheet);
            _notificationService.ShowNotification(new Notification(
                "Sheet imported.",
                $"Successfully imported \"{sheet.Name}\".",
                NotificationType.Success));
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
        await _sheetStorage.RemoveSheetAsync(_selectedSheet);
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
        var newSheet = await ShowRenameDialog.Handle(_selectedSheet);
        if (newSheet != _selectedSheet)
        {
            await _sheetStorage.RenameSheetAsync(SelectedSheet.Name, newSheet);
            _sheetSource.Remove(SelectedSheet);
            _sheetSource.AddOrUpdate(newSheet);
            //SelectedSheet.RenameSheet(dialogResult);
        }
    }

    [ReactiveCommand]
    private async Task EditSheet()
    {
        var editResult = await ShowEditDialog.Handle(SelectedSheet);
        if (editResult != null)
        {
            await _sheetStorage.UpdateSheetAsync(editResult);
            _sheetSource.AddOrUpdate(editResult);
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
        manualVm.ChooseSheet(SelectedSheet);
    }

    [ReactiveCommand]
    private async Task DuplicateSheet()
    {
        var original = SelectedSheet;
        var allNames = _sheetSource.Items.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newName = SheetStorage.GenerateCopyName(original.Name, allNames);

        // Duplicate the object (assuming Sheet is immutable or cloneable)
        var duplicated = original.RenameSheet(newName, original.Description);

        await _sheetStorage.SaveSheetAsync(duplicated);
        _sheetSource.AddOrUpdate(duplicated);

        _notificationService.ShowNotification(new Notification("Sheet duplicated.",
            $"A copy named \"{newName}\" was created.",
            NotificationType.Success));
    }

    private async Task LoadSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        _sheetSource.Clear();
        _sheetSource.AddOrUpdate(sheets);
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
    ReadOnlyObservableCollection<Sheet> Sheets { get; }
    ReactiveCommand<Unit, Unit> RemoveSheetCommand { get; }
    ReactiveCommand<Unit, Unit> RenameSheetCommand { get; }
    ReactiveCommand<Unit, Unit> EditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> ManuallyEditSheetCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToRecordingViewCommand { get; }
    ReactiveCommand<Unit, Unit> NavigateToManualViewCommand { get; }
    Sheet? SelectedSheet { get; set; }
    Interaction<Sheet, Sheet?> ShowEditDialog { get; }
    Interaction<Sheet, Sheet> ShowRenameDialog { get; }
    Interaction<(Sheet, Sheet), Unit> ShowCompareDialog { get; }
    Interaction<ConfirmationViewModel, Confirmation> ShowConfirmationDialog { get; }
    ReactiveCommand<Unit, Unit> DuplicateSheetCommand { get; }
    bool SheetExists(string sheetName);
    Task SaveSheet(Sheet sheet);
    Task SaveSelectedSheetAs(SaveFormat format);
    Task CompareSheets(Sheet baseSheet, Sheet comparedSheet);
    Task BatchRemoveSheets(List<Sheet> selected);
}