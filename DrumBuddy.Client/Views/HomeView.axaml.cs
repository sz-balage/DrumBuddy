using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.Models;
using DrumBuddy.Client.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            // Bind ItemsSource
            this.OneWayBind(ViewModel,
                    vm => vm.Cards,
                    v => v.CardsItemsControl.ItemsSource)
                .DisposeWith(d);
        });
    }
}