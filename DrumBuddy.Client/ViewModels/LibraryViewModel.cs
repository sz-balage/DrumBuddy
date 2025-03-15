using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Core.Models;
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

    [Reactive] private Sheet _selectedSheet;

    public LibraryViewModel()
    {
        HostScreen = Locator.Current.GetRequiredService<IScreen>();
        _sheetSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null);
    }

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }

    public void AddSheet(Sheet sheet)
    {
        _sheetSource.AddOrUpdate(sheet);
    }

    [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
    private void RemoveSheet()
    {
        _sheetSource.Remove(_selectedSheet);
    }
}