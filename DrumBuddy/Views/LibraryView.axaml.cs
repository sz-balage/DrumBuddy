using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.DesignHelpers;
using DrumBuddy.Extensions;
using DrumBuddy.IO.Services;
using DrumBuddy.Models;
using DrumBuddy.Services;
using DrumBuddy.ViewModels;
using DrumBuddy.ViewModels.Dialogs;
using DrumBuddy.Views.Dialogs;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views;

public partial class LibraryView : ReactiveUserControl<ILibraryViewModel>
{
    private readonly ConfigurationService _configurationService;
    private readonly MainWindow _mainWindow;
    private readonly MidiService _midiService;
    private readonly PdfGenerator _pdfGenerator;

    public LibraryView()
    {
        if (!Design.IsDesignMode)
        {
            _midiService = Locator.Current.GetRequiredService<MidiService>();
            _configurationService = Locator.Current.GetRequiredService<ConfigurationService>();
            _pdfGenerator = Locator.Current.GetRequiredService<PdfGenerator>();
            _mainWindow = Locator.Current.GetService<MainWindow>();
        }
        else
        {
            ViewModel = new DesignLibraryViewModel();
        }

        InitializeComponent();

        this.WhenActivated(async d =>
        {
            this.OneWayBind(ViewModel,
                    vm => vm.Sheets,
                    v => v.SheetsLB.ItemsSource)
                .DisposeWith(d);
            if (Design.IsDesignMode)
                return;
            this.OneWayBind(ViewModel,
                    vm => vm.SelectedSheet,
                    v => v.BatchDeleteMenuItem.IsEnabled, b => b != null)
                .DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SelectedSheet,
                    v => v.SheetsLB.SelectedItem)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.ZeroStateGrid.IsVisible, count => count == 0)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.ZeroStateTextBlock.IsVisible, count => count == 0)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.ZeroStateImportSheetButton.IsVisible,
                    count => count == 0)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.SelectAllButton.IsVisible, count => count != 0)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets.Count, v => v.BatchDeleteMenuItem.IsVisible, count => count != 0)
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
            this.BindInteraction(ViewModel, vm => vm.ShowConfirmationDialog, ConfirmationHandler)
                .DisposeWith(d);
        });
    }

    private IEnumerable<Sheet> SelectedSheets => SheetsLB.SelectedItems.Cast<Sheet>();

    private ListBox SheetsLB => this.FindControl<ListBox>("SheetsListBox");
    private Button NavSheetButton => this.FindControl<Button>("CreateFirstSheetButton");
    private UniformGrid ZeroStateGrid => this.FindControl<UniformGrid>("ZeroStateStack");

    private async Task ConfirmationHandler(IInteractionContext<ConfirmationViewModel, Confirmation> context)
    {
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var saveView = new ConfirmationView
        {
            ViewModel = context.Input
        };
        var result = await saveView.ShowDialog<Confirmation>(mainWindow);
        context.SetOutput(result);
    }

    private async Task HandleCompare(IInteractionContext<(Sheet, Sheet), Unit> arg)
    {
        var view = new CompareView { ViewModel = new CompareViewModel(arg.Input) };
        await view.ShowDialog(_mainWindow);
        arg.SetOutput(Unit.Default);
    }

    private async Task HandleEdit(IInteractionContext<Sheet, Sheet?> arg)
    {
        var view = new EditingView
            { ViewModel = new EditingViewModel(arg.Input, _midiService, _configurationService, _pdfGenerator) };
        var result = await view.ShowDialog<Sheet?>(_mainWindow);
        arg.SetOutput(result);
    }

    private async Task HandleRename(IInteractionContext<Sheet, Sheet> arg)
    {
        var view = new RenameSheetView { ViewModel = new RenameSheetViewModel(arg.Input) };
        var result = await view.ShowDialog<Sheet>(_mainWindow);
        arg.SetOutput(result);
    }

    private void CompareButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not Sheet baseSheet) return;
        if (ViewModel is null) return;
        var flyout = new MenuFlyout();
        var otherSheets = ViewModel.Sheets.Where(s => s != baseSheet).ToList();
        if (!otherSheets.Any())
            flyout.Items.Add(new MenuItem { Header = "No other sheets to compare with", IsEnabled = false });
        else
        {
            foreach (var sheet in otherSheets)
            {
                var menuItem = new MenuItem { Header = sheet.Name, Tag = sheet };
                menuItem.Click += async (s, args) =>
                {
                    if (s is MenuItem mi && mi.Tag is Sheet selectedSheet)
                        await ViewModel.CompareSheets(baseSheet, selectedSheet);
                };
                flyout.Items.Add(menuItem);
            }
        }

        FlyoutBase.SetAttachedFlyout(button, flyout);
        flyout.ShowAt(button);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Parent is Grid grid &&
            grid.Parent is ListBoxItem item)
            SheetsListBox.SelectedItem = item.DataContext;
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

    private void DuplicateMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.DuplicateSheetCommand.Execute().Subscribe();
    }

    private void ViewButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Parent is Grid grid &&
            grid.Parent is ListBoxItem item)
            SheetsListBox.SelectedItem = item.DataContext;
        var mainWindow = Locator.Current.GetService<MainWindow>();
        var view = new EditingView
        {
            ViewModel = new EditingViewModel(ViewModel.SelectedSheet, _midiService, _configurationService,
                _pdfGenerator, true)
        };
        view.Show(mainWindow);
    }

    private void SelectAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SheetsLB.Items is null)
            return;

        // Select all sheets if not already all selected, otherwise clear selection
        var allItems = SheetsLB.Items.Cast<object>().ToList();
        var selectedCount = SheetsLB.SelectedItems?.Count ?? 0;

        if (selectedCount < allItems.Count)
        {
            SheetsLB.SelectedItems.Clear();
            foreach (var item in allItems)
                SheetsLB.SelectedItems.Add(item);
        }
        else
        {
            SheetsLB.SelectedItems.Clear();
        }
    }

    private void BatchDeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;
        var selected = SheetsLB.SelectedItems.Cast<Sheet>().ToList();
        if (selected.Count > 0) _ = ViewModel.BatchRemoveSheets(selected);
    }

    private void ManualEdit(object? sender, RoutedEventArgs e)
    {
        ViewModel.ManuallyEditSheetCommand.Execute().Subscribe();
    }

    private void SaveSheetAs(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Parent is Grid grid &&
            grid.Parent is ListBoxItem item)
            SheetsListBox.SelectedItem = item.DataContext;
        ViewModel.SaveSelectedSheetAsCommand.Execute().Subscribe();
    }
}