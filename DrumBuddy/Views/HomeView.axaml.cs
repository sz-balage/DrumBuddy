using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;

namespace DrumBuddy.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
        // this.WhenActivated(d =>
        // {
        //     // Bind ItemsSource
        //     this.OneWayBind(ViewModel,
        //             vm => vm.Cards,
        //             v => v.CardsItemsControl.ItemsSource)
        //         .DisposeWith(d);
        // });
    }
}