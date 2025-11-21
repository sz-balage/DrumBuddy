using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Extensions;
using DrumBuddy.ViewModels;
using ReactiveUI;
using Splat;

namespace DrumBuddy.Views;

public partial class ConfigurationView : ReactiveUserControl<ConfigurationViewModel>
{
    public ConfigurationView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            if (Design.IsDesignMode)
                return;
            ViewModel?.ListeningDrumChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(drum =>
                {
                    if (drum is null)
                        ListeningDrumText.Text = "Not Listening";
                    else
                        ListeningDrumText.Text = $"Listening for: {drum}";
                })
                .DisposeWith(d);

            this.Bind(ViewModel,
                    vm => vm.KeyboardInput,
                    v => v.InputModeToggle.IsChecked) // Changed from KeyboardInputCheckBox
                .DisposeWith(d);
            ViewModel.WhenAnyValue(x => x.KeyboardInput)
                .Subscribe(TriggerKeyboardAndMidiForegrounds);
            // this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.MIDIModeText.Foreground, ki => ki ? Brushes.Gray : NoteColor);        
            // this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.MIDIModeIcon.Foreground, ki => ki ? Brushes.Gray : NoteColor);     
            // this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.KeyboardModeText.Foreground, ki => ki ? NoteColor : Brushes.Gray);  
            // this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.KeyboardModeIcon.Foreground, ki => ki ? NoteColor : Brushes.Gray);
            this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.RevertTextBlock.Text,
                ki => ki ? "Revert keyboard mappings" : "Revert drum mappings");
            ViewModel.WhenAnyValue(vm => vm.SelectedThemeMode).Subscribe(_ =>
            {
                TriggerKeyboardAndMidiForegrounds(ViewModel.KeyboardInput);
            });
            var mainView = Locator.Current.GetRequiredService<MainWindow>();
            ViewModel.KeyboardBeats = mainView.KeyboardBeats;
            ViewModel.DrumMappingTabSelected = true;
            DrumMappingTab.PointerPressed += DrumMappingTab_PointerPressed;
            SettingsTab.PointerPressed += SettingsTab_PointerPressed;
        });
    }

    private static SolidColorBrush NoteColor => new((Color)App.Current?.FindResource("NoteColor"));

    private void TriggerKeyboardAndMidiForegrounds(bool keyboardInput)
    {
        if (keyboardInput)
        {
            MIDIModeText.Foreground = Brushes.Gray;
            MIDIModeIcon.Foreground = Brushes.Gray;
            KeyboardModeText.Foreground = NoteColor;
            KeyboardModeIcon.Foreground = NoteColor;
        }
        else
        {
            MIDIModeText.Foreground = NoteColor;
            MIDIModeIcon.Foreground = NoteColor;
            KeyboardModeText.Foreground = Brushes.Gray;
            KeyboardModeIcon.Foreground = Brushes.Gray;
        }
    }

    private void DrumMappingTab_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ViewModel.DrumMappingTabSelected = true;
    }

    private void SettingsTab_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ViewModel.DrumMappingTabSelected = false;
    }
}