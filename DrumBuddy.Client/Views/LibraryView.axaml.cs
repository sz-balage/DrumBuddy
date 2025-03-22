using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.DesignHelpers;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Client.ViewModels.Dialogs;
using DrumBuddy.Core.Models;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Client.Views;

public partial class LibraryView : ReactiveUserControl<ILibraryViewModel>
{
    public LibraryView()
    {
        if(Design.IsDesignMode)
            ViewModel = new DesignLibraryViewModel();
        InitializeComponent();
        // if (Design.IsDesignMode)
        // {
        //     SheetsLB.Items.Add(new Sheet(new Bpm(100), ImmutableArray<Measure>.Empty, "New Sheet", "New Sheet"));
        // }
        
        this.WhenActivated(async d =>
        {
            //TODO: look at why design vm still dont work
           
            this.OneWayBind(ViewModel,
                    vm => vm.Sheets,
                    v => v.SheetsLB.ItemsSource)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SelectedSheet,
                    v => v.SheetsLB.SelectedItem)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.SheetsStackPanel.IsVisible, count => count == 0)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.NavigateToRecordingViewCommand, view => view.NavSheetButton)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowRenameDialog, HandleRename)
                .DisposeWith(d);
            // this.BindCommand(ViewModel, vm => vm.RemoveSheetCommand,
            //         v => v.DeleteSheetMenuItem)
            //     .DisposeWith(d);
        });
    }

    private async Task HandleRename(IInteractionContext<Sheet, string?> arg)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new Dialogs.RenameSheetView(){ViewModel = new RenameSheetViewModel(arg.Input)};
        var result = await view.ShowDialog<string>(mainWindow);
        arg.SetOutput(result);
    }

    private ListBox SheetsLB => this.FindControl<ListBox>("SheetsListBox");
    private Button NavSheetButton => this.FindControl<Button>("CreateFirstSheetButton");
    private StackPanel SheetsStackPanel => this.FindControl<StackPanel>("ZeroStateStack");

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
       
        if (sender is Button button && 
            button.Parent is Grid grid && 
            grid.Parent is ListBoxItem item)
        {
            // Select the ListBoxItem
            SheetsListBox.SelectedItem = item.DataContext;
        }
    }

    private void DeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSheetCommand.Execute().Subscribe();
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.RenameSheetCommand.Execute().Subscribe();
    }
}