using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
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
                    v => v.KeyboardInputCheckBox.IsChecked)
                .DisposeWith(d);
            var mainView = Locator.Current.GetRequiredService<MainWindow>();
            ViewModel.KeyboardBeats = mainView.KeyboardBeats;
            ViewModel.DrumMappingTabSelected = true;
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