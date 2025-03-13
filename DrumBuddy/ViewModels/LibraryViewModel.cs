using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class LibraryViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly ReadOnlyObservableCollection<Sheet> _sheets;
    private readonly SourceCache<Sheet, string> _sheetSource = new(s => s.Name);
    private IObservable<bool> _removeCanExecute;

    public LibraryViewModel()
    {
        HostScreen = Locator.Current.GetRequiredService<IScreen>();
        _sheetSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _sheets)
            .Subscribe();
        _removeCanExecute = this.WhenAnyValue(vm => vm.SelectedSheet).Select(sheet => sheet != null);
    }

    public void AddSheet(Sheet sheet)
    {
        _sheetSource.AddOrUpdate(sheet);
    }

    [Reactive] private Sheet _selectedSheet;

    [ReactiveCommand(CanExecute = nameof(_removeCanExecute))]
    private void RemoveSheet()
    {
        _sheetSource.Remove(_selectedSheet);
    }

    public ReadOnlyObservableCollection<Sheet> Sheets => _sheets;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }
}