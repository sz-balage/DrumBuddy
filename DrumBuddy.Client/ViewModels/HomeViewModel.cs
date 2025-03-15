using DrumBuddy.Client.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    [Reactive] private string _subText = "To continue, please select an option from the menu on the left.";
    [Reactive] private string _welcomeText = "Welcome to DrumBuddy.Client!";

    public string? UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; } = Locator.Current.GetRequiredService<IScreen>();
}