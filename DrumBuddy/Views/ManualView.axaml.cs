using System;
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
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.CardGrid.IsVisible, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.EditorVisible, v => v.EditorPlaceHolder.IsVisible)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.SheetChooserStackPanel.IsVisible)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.AddButton.IsEnabled, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SheetListVisible, v => v.EditButton.IsVisible, b => !b)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.Sheets, v => v.SheetListBox.ItemsSource)
                .DisposeWith(d);
            ViewModel.WhenAnyValue(x => x.Editor).Subscribe(vm =>
            {
                if (vm == null)
                    return;
                EditorPlaceHolder.Children.Clear();
                EditorPlaceHolder.Children.Add(new ManualEditorView
                {
                    ViewModel = vm
                });
            });
            this.BindCommand(ViewModel, vm => vm.AddNewSheetCommand, v => v.AddButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.EditExistingSheetCommand, v => v.EditButton)
                .DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.CancelSheetChoosingCommand, v => v.CancelSheetChoosingButton)
                .DisposeWith(d);
            _ = ViewModel.LoadExistingSheets();
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