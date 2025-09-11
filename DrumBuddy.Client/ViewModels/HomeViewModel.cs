using DrumBuddy.Client.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace DrumBuddy.Client.ViewModels;

public partial class HomeViewModel : ReactiveObject, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = "home";
    public IScreen HostScreen { get; } = Locator.Current.GetRequiredService<IScreen>();
}