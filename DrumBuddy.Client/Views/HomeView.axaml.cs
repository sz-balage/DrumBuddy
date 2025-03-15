using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Client;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.WelcomeText, v => v.WelcomeLabel.Content)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SubText, v => v.SubTextLabel.Content)
                .DisposeWith(d);
        });
    }
}