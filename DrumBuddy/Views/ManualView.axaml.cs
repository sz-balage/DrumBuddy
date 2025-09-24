using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Models;
using DrumBuddy.ViewModels;
using ReactiveUI;

namespace DrumBuddy.Views;

public partial class ManualView : ReactiveUserControl<ManualViewModel>
{
    public ManualView()
    {
        InitializeComponent();

        this.WhenActivated(async d =>
        {
            await ViewModel.LoadExistingSheets();
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.CardGrid.IsVisible, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.EditorView.IsVisible)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.SheetChooserStackPanel.IsVisible)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.AddButton.IsEnabled, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.EditButton.IsVisible, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets, v => v.SheetListBox.ItemsSource)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Editor, v => v.EditorView.ViewModel)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.AddNewSheetCommand, v => v.AddButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.EditExistingSheetCommand, v => v.EditButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.CancelSheetChoosingCommand, v => v.CancelSheetChoosingButton)
                .DisposeWith(d);
        });
    }

    private void SheetListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is null) return;

        if (SheetListBox.SelectedItem is Sheet selectedSheet)
        {
            ViewModel.ChooseSheet(selectedSheet);
            SheetListBox.SelectedItem = null;
        }
    }
}