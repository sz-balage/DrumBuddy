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
    private Button DeleteSheetButton => this.FindControl<Button>("DeleteButton");
    public LibraryView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel,
                     vm => vm.Sheets,
                      v => v.SheetsLB.ItemsSource)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SelectedSheet,
                      v => v.SheetsLB.SelectedItem)
                .DisposeWith(d);
            
            this.BindCommand(ViewModel, vm => vm.RemoveSheetCommand,
                                     v => v.DeleteSheetButton)
                .DisposeWith(d);
        });
    }
}