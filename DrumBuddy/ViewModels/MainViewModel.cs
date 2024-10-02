using ReactiveUI;

namespace DrumBuddy.ViewModels
{
    public class MainViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new();
        public MainViewModel()
        {
            Router.Navigate.Execute(new RecordingViewModel(this));
        }
    }
}
