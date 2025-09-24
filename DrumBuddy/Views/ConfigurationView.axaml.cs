using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Core.Enums;
using DrumBuddy.ViewModels;
using ReactiveUI;

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
                    {
                        ListeningDrumText.Text = "Not Listening";
                    }
                    else
                    {
                        ListeningDrumText.Text = $"Listening for: {drum}";
                    }
                })
                .DisposeWith(d);
            this.Bind(ViewModel,
                    vm => vm.KeyboardInput,
                    v => v.KeyboardInputCheckBox.IsChecked)
                .DisposeWith(d);

            ViewModel.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                .Select(ep => ep.EventArgs as KeyEventArgs)
                .Select(e => e.Key switch
                {
                    Key.A => (int)Drum.HiHat,
                    Key.S => (int)Drum.Snare,
                    Key.D => (int)Drum.Kick,
                    Key.F => (int)Drum.FloorTom,
                    Key.Q => (int)Drum.Crash,
                    Key.W => (int)Drum.Tom1,
                    Key.E => (int)Drum.Tom2,
                    Key.R => (int)Drum.Ride,
                    _ => -2
                });
        });
    }
}