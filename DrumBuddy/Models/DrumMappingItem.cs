using DrumBuddy.Core.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Models;

public partial class DrumMappingItem : ReactiveObject
{
    [Reactive] private bool _isHighlighted;

    [Reactive] private bool _isListening;

    private int _note;

    public DrumMappingItem(Drum drum, int note)
    {
        Drum = drum;
        _note = note;
    }

    public Drum Drum { get; }
    public string DrumName => Drum.ToString();

    public int Note
    {
        get => _note;
        set
        {
            this.RaiseAndSetIfChanged(ref _note, value);
            this.RaisePropertyChanged(nameof(IsUnmapped));
        }
    }

    public bool IsUnmapped => Note == -1;
}