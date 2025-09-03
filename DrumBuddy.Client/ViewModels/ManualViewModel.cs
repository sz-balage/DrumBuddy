using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Abstractions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Client.ViewModels;
public enum ManualMode
{
    Choice,   //first screen
    Create,   //go to editor
    Edit      //list of existing sheets
}
public partial class ManualViewModel : ReactiveObject,IRoutableViewModel
{
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private readonly ISheetStorage _sheetStorage;
    
   
    public ManualViewModel(IScreen host, ISheetStorage sheetStorage)
    {
        HostScreen = host;
        _sheetStorage = sheetStorage;
        _sheetSource.Connect()
            .SortBy(s => s.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        this.WhenNavigatedTo(() => LoadSheets()
            .ToObservable()
            .Subscribe());
    }
    [Reactive]
    private ManualMode _mode = ManualMode.Choice;
    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    private async Task LoadSheets()
    {
        var sheets = await _sheetStorage.LoadSheetsAsync();
        _sheetSource.Clear();
        _sheetSource.AddOrUpdate(sheets);
    }
    [ReactiveCommand]
    private void GoToCreate() => Mode = ManualMode.Create;
    [ReactiveCommand]
    private void GoToEdit() => Mode = ManualMode.Edit;
}