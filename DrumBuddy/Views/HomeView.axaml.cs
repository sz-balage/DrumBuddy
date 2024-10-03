using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;

namespace DrumBuddy;

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