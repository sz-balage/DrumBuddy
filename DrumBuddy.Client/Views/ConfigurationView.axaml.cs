using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DrumBuddy.Client.ViewModels;
using DrumBuddy.Core.Enums;
using ReactiveUI;

namespace DrumBuddy.Client.Views;

public partial class ConfigurationView : ReactiveUserControl<ConfigurationViewModel>
{
    public ConfigurationView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            ViewModel?.ListeningDrumChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(drum =>
                {
                    if (drum is null)
                    {
                        ListeningDrumText.Text = "Not Listening";
                        CancelButton.IsVisible = false;
                    }
                    else
                    {
                        ListeningDrumText.Text = $"Listening for: {drum}";
                        CancelButton.IsVisible = true;
                    }
                })
                .DisposeWith(d);
            this.Bind(ViewModel,
                    vm => vm.KeyboardInput,
                    v => v.KeyboardInputCheckBox.IsChecked)
                .DisposeWith(d);

            this.BindCommand(ViewModel,
                    vm => vm.StopListeningCommand,
                    v => v.CancelButton)
                .DisposeWith(d);
            
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.KickButton, 
                Observable.Return(Drum.Kick)).DisposeWith(d);   
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.SnareButton, 
                    Observable.Return(Drum.Snare)).DisposeWith(d);     
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.Tom1Button,
                    Observable.Return(Drum.Tom1)).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.Tom2Button,
                    Observable.Return(Drum.Tom2)).DisposeWith(d);  
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.FloorTomButton,
                    Observable.Return(Drum.FloorTom)).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.RideButton,
                    Observable.Return(Drum.Ride)).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.CrashButton,
                    Observable.Return(Drum.Crash)).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartListeningCommand, v => v.HiHatButton,
                    Observable.Return(Drum.HiHat)).DisposeWith(d);
            ViewModel?.HighlightDrum.Subscribe(HighlightDrum)
                .DisposeWith(d);
            ViewModel.MappingChanged.Subscribe(_ => UpdateDrumButtonHighlights()).DisposeWith(d);
            UpdateDrumButtonHighlights();
            ViewModel!.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
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

    private async void HighlightDrum(Drum drum)
    {
        var button = GetButtonForDrum(drum);
        if (button is null) return;

        button.Classes.Add("Highlight");
        await Task.Delay(500);
        button.Classes.Remove("Highlight");
    }
    private void UpdateDrumButtonHighlights()
    {
        var drumButtons = new Dictionary<Drum, Button>
        {
            { Drum.Kick, KickButton },
            { Drum.Snare, SnareButton },
            { Drum.HiHat, HiHatButton },
            { Drum.Tom1, Tom1Button },
            { Drum.Tom2, Tom2Button },
            { Drum.FloorTom, FloorTomButton },
            { Drum.Ride, RideButton },
            { Drum.Crash, CrashButton }
        };

        foreach (var kvp in drumButtons)
        {
            var drum = kvp.Key;
            var button = kvp.Value;

            button.Classes.Remove("Unmapped");
            button.Classes.Remove("Highlight");

            if (ViewModel.Mapping.TryGetValue(drum, out var mappedNote))
            {
                if (mappedNote == -1)
                    button.Classes.Add("Unmapped"); // red background for unmapped
            }
            else
            {
                button.Classes.Add("Unmapped");
            }
        }
    }
    private Button? GetButtonForDrum(Drum drum) => drum switch
    {
        Drum.Kick => KickButton,
        Drum.Snare => SnareButton,
        Drum.HiHat => HiHatButton,
        Drum.Tom1 => Tom1Button,
        Drum.Tom2 => Tom2Button,
        Drum.FloorTom => FloorTomButton,
        Drum.Ride => RideButton,
        Drum.Crash => CrashButton,
        _ => null
    };
}