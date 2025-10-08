using System.Collections.ObjectModel;
using DrumBuddy.IO.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class MidiDeviceChooserViewModel(MidiDeviceShortInfo[] allDevices) : ReactiveObject
{
    [Reactive] private MidiDeviceShortInfo? _selectedMidiDevice;

    public ObservableCollection<MidiDeviceShortInfo> MidiDevices { get; set; } = new(allDevices);
}