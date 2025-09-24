using System.Collections.ObjectModel;
using System.Linq;
using DrumBuddy.Extensions;
using DrumBuddy.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using ReactiveUI;
using Splat;

namespace DrumBuddy.ViewModels;

public class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly MainViewModel _mainVm;

    public HomeViewModel()
    {
        _mainVm = Locator.Current.GetRequiredService<MainViewModel>();
        Cards = new ObservableCollection<NavigationCardViewModel>(
            _mainVm.PaneItems.Skip(1) //skip home
                .Select(template => new NavigationCardViewModel(template, Navigate))
        );
    }

    public ObservableCollection<NavigationCardViewModel> Cards { get; }
    public string? UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; } = Locator.Current.GetRequiredService<IScreen>();

    public void Navigate(NavigationMenuItemTemplate item)
    {
        _mainVm.NavigateFromCode(Locator.Current.GetRequiredService(item.ModelType) as IRoutableViewModel);
    }
}