using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.DesignHelpers;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Client.ViewModels.Dialogs;
using DrumBuddy.Client.Views.Dialogs;
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
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.ZeroStateGrid.IsVisible, count => count == 0)
                .DisposeWith(d);       
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.ZeroStateTextBlock.IsVisible, count => count == 0)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.NavigateToRecordingViewCommand, view => view.RecordButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.NavigateToManualViewCommand, view => view.ManualButton)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowRenameDialog, HandleRename)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowEditDialog, HandleEdit)
                .DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.ShowCompareDialog, HandleCompare)
                .DisposeWith(d);
            
            // this.BindCommand(ViewModel, vm => vm.RemoveSheetCommand,
            //         v => v.DeleteSheetMenuItem)
            //     .DisposeWith(d);
        });
    }

    private async Task HandleCompare(IInteractionContext<(Sheet, Sheet), Unit> arg)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new CompareView(){ViewModel = new CompareViewModel(arg.Input)};
        await view.ShowDialog(mainWindow);
        arg.SetOutput(Unit.Default);
    }
    private async Task HandleEdit(IInteractionContext<Sheet, Sheet?> arg)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new EditingView(){ViewModel = new EditingViewModel(arg.Input)};
        var result = await view.ShowDialog<Sheet?>(mainWindow);
        arg.SetOutput(result);
    }
    private async Task HandleRename(IInteractionContext<Sheet, Sheet> arg)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new RenameSheetView(){ViewModel = new RenameSheetViewModel(arg.Input)};
        var result = await view.ShowDialog<Sheet>(mainWindow);
        arg.SetOutput(result);
    }
    private void CompareButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not Sheet baseSheet) return;
        if (ViewModel is null) return;
        var flyout = new MenuFlyout();

        foreach (var sheet in ViewModel.Sheets)
        {
            if (sheet == baseSheet) continue; // don't compare with itself
            var menuItem = new MenuItem { Header = sheet.Name, Tag = sheet };
            menuItem.Click += async (s, args) =>
            {
                if (s is MenuItem mi && mi.Tag is Sheet selectedSheet)
                {
                    await ViewModel.CompareSheets(baseSheet, selectedSheet);
                }
            };
            flyout.Items.Add(menuItem);
        }
        FlyoutBase.SetAttachedFlyout(button, flyout);
        flyout.ShowAt(button);
    }

    private ListBox SheetsLB => this.FindControl<ListBox>("SheetsListBox");
    private Button NavSheetButton => this.FindControl<Button>("CreateFirstSheetButton");
    private UniformGrid ZeroStateGrid => this.FindControl<UniformGrid>("ZeroStateStack");

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
       
        if (sender is Button button && 
            button.Parent is Grid grid && 
            grid.Parent is ListBoxItem item)
        {
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

    private void EditMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.EditSheetCommand.Execute().Subscribe();
    }

    private void ViewButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Parent is Grid grid &&
            grid.Parent is ListBoxItem item)
            SheetsListBox.SelectedItem = item.DataContext;
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new EditingView { ViewModel = new EditingViewModel(ViewModel.SelectedSheet, true) };
        view.Show(mainWindow);
    }
}