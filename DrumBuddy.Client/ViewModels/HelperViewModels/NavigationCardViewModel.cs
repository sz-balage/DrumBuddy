using System;
using System.Reactive;
using DrumBuddy.Client.Models;
using ReactiveUI;

namespace DrumBuddy.Client.ViewModels.HelperViewModels;

public class NavigationCardViewModel : ReactiveObject
{
    public NavigationCardViewModel(NavigationMenuItemTemplate template, Action<NavigationMenuItemTemplate> navigateAction)
    {
        Template = template;
        NavigateCommand = ReactiveCommand.Create(() => navigateAction(template));
    }

    public NavigationMenuItemTemplate Template { get; }

    public ReactiveCommand<Unit, Unit> NavigateCommand { get; }
}