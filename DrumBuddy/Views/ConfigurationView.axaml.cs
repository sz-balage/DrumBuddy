using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using DrumBuddy.Services;
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
                        ListeningDrumText.Text = "Not Listening";
                    else
                        ListeningDrumText.Text = $"Listening for: {drum}";
                })
                .DisposeWith(d);
            this.Bind(ViewModel,
                    vm => vm.KeyboardInput,
                    v => v.KeyboardInputCheckBox.IsChecked)
                .DisposeWith(d);

            ViewModel.KeyboardBeats = Observable.FromEventPattern(this, nameof(KeyDown))
                .Select(ep => ep.EventArgs as KeyEventArgs)
                .Select(e => KeyboardBeatProvider.GetDrumValueForKey(e.Key));
        });
    }
}