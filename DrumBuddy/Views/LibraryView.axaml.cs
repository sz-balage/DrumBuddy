using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.ViewModels;
using ReactiveUI;

namespace DrumBuddy;

public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    private ListBox SheetsLB => this.FindControl<ListBox>("SheetsListBox");

    public LibraryView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                     vm => vm.Sheets,
                      v => v.SheetsLB.ItemsSource)
                .DisposeWith(d);
        });
    }
}