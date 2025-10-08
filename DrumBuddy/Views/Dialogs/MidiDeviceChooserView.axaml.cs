using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using DrumBuddy.IO.Services;
using DrumBuddy.ViewModels.Dialogs;
using ReactiveUI;

namespace DrumBuddy.Views.Dialogs;

public class MidiDeviceChooserView : ReactiveWindow<MidiDeviceChooserViewModel>
{
    public MidiDeviceChooserView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            ViewModel = new MidiDeviceChooserViewModel([
                new MidiDeviceShortInfo(0, "Test Device 1"), new MidiDeviceShortInfo(1, "Test Device 2")
            ]);
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.SelectedMidiDevice).Subscribe(md =>
            {
                ChooseButton.IsEnabled = md is not null;
            });
            Closing += (sender, args) =>
            {
                if (!args.IsProgrammatic) Close(null);
            };
        });
    }

    private void ChooseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.SelectedMidiDevice is not null) Close(ViewModel.SelectedMidiDevice);
    }
}