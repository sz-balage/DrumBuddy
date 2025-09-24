using System;
using System.Reactive;
using DrumBuddy.Models;
using ReactiveUI;

namespace DrumBuddy.ViewModels.HelperViewModels;

public class NavigationCardViewModel : ReactiveObject
{
    public NavigationCardViewModel(NavigationMenuItemTemplate template,
        Action<NavigationMenuItemTemplate> navigateAction)
    {
        Template = template;
        NavigateCommand = ReactiveCommand.Create(() => navigateAction(template));
    }

    public NavigationMenuItemTemplate Template { get; }

    public ReactiveCommand<Unit, Unit> NavigateCommand { get; }
}