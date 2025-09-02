using DrumBuddy.IO.Abstractions;
using ReactiveUI;

namespace DrumBuddy.Client.ViewModels;

public class ManualViewModel : ReactiveObject,IRoutableViewModel
{
    private readonly ISheetStorage _sheetStorage;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }

    public ManualViewModel(IScreen host, ISheetStorage sheetStorage)
    {
        HostScreen = host;
        _sheetStorage = sheetStorage;
    }
}