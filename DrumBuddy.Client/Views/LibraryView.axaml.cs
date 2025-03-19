using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class LibraryView : ReactiveUserControl<LibraryViewModel>
{
    public LibraryView()
    {
        InitializeComponent();
        this.WhenActivated(async d =>
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

    private ListBox SheetsLB => this.FindControl<ListBox>("SheetsListBox");
    private Button DeleteSheetButton => this.FindControl<Button>("DeleteButton");
}