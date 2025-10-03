using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DrumBuddy.Extensions;
using DrumBuddy.Services;
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
            this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.MIDIModeText.Foreground, ki =>
            {
                return ki ? Brushes.Gray : Brushes.Black;
            });        
            this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.MIDIModeIcon.Foreground, ki =>
            {
                return ki ? Brushes.Gray : Brushes.Black;
            });     
            this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.KeyboardModeText.Foreground, ki =>
            {
                return ki ? Brushes.Black : Brushes.Gray;
            });  
            this.OneWayBind(ViewModel, vm => vm.KeyboardInput, v => v.KeyboardModeIcon.Foreground, ki =>
            {
                return ki ? Brushes.Black : Brushes.Gray;
            });
            var mainView = Locator.Current.GetRequiredService<MainWindow>();
            ViewModel.KeyboardBeats = mainView.KeyboardBeats;
            ViewModel.DrumMappingTabSelected = true;
            DrumMappingTab.PointerPressed += DrumMappingTab_PointerPressed;
            SettingsTab.PointerPressed += SettingsTab_PointerPressed;
        });
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