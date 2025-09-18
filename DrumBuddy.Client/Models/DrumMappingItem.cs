using DrumBuddy.Core.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Client.Models;
public partial class DrumMappingItem : ReactiveObject
{
    public DrumMappingItem(Drum drum, int note)
    {
        Drum = drum;
        _note = note;
    }

    public Drum Drum { get; }
    public string DrumName => Drum.ToString();

    private int _note;
    public int Note
    {
        get => _note;
        set
        {
            this.RaiseAndSetIfChanged(ref _note, value);
            this.RaisePropertyChanged(nameof(IsUnmapped)); 
        }
    }
    [Reactive]
    private bool _isHighlighted;   
    [Reactive]
    private bool _isListening;

    public bool IsUnmapped => Note == -1;
}