using DrumBuddy.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.ViewModels;

public partial class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    [Reactive] private string _welcomeText = "Welcome to DrumBuddy!";
    [Reactive] private string _subText = "To continue, please select an option from the menu on the left.";

    public string? UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; } = Locator.Current.GetRequiredService<IScreen>();
}