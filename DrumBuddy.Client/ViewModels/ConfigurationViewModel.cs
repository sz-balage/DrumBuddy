using ReactiveUI;

namespace DrumBuddy.Client.ViewModels;

public partial class ConfigurationViewModel : ReactiveObject, IRoutableViewModel
{
    public ConfigurationViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
}