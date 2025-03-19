using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Abstractions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class LibraryViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private IObservable<bool> _removeCanExecute;
    private ISheetStorage _sheetStorage;
    public LibraryViewModel(IScreen hostScreen, ISheetStorage sheetStorage)
    {
        HostScreen = hostScreen;
        _sheetStorage = sheetStorage;
        _sheetSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null!);
        this.WhenNavigatedTo(() =>
        {
            return LoadSheets()
                .ToObservable()
                .Subscribe(_ =>
                    {
                        var source = _sheetSource;
                        ;
                    },
                    onError: ex => Debug.WriteLine(ex));
        });
    }
    [Reactive] private Sheet _selectedSheet;

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public async Task TrySaveSheet(Sheet sheet)
    {
        await _sheetStorage.SaveSheetAsync(sheet);
    }
    
    //TODO: implement rename sheet
    
    [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
    private async Task RemoveSheet()
    {
        await _sheetStorage.RemoveSheetAsync(_selectedSheet);
        _sheetSource.Remove(_selectedSheet);
    }
    public async Task LoadSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        _sheetSource.Clear();
        _sheetSource.AddOrUpdate(sheets);
    }

    public bool SheetExists(string sheetName)
    {
        return _sheetStorage.SheetExists(sheetName);
    }
}