using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DynamicData;
using LanguageExt;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.HelperViewModels
{
    public partial class MeasureViewModel : ReactiveObject
    {

        private double _pointerPosition;
        public double PointerPosition
        {
            get => _pointerPosition;
            set => this.RaiseAndSetIfChanged(ref _pointerPosition, value);
        }
        public Measure Measure = new(new(4));
        public MeasureViewModel()
        {
            IsPointerVisible = true;
        }
        [Reactive]
        private bool _isPointerVisible;
        public void AddRythmicGroupFromNotes(List<NoteGroup> notes)
        {
            var rg = new RythmicGroup(RecordingService.UpscaleNotes(notes).ToImmutableArray()); //will be a call to the recordingservice
            Measure.Groups.Add(rg);
            RythmicGroups.Add(new(rg));
        }

        public bool IsEmpty => Measure.IsEmpty;
        public ObservableCollection<RythmicGroupViewModel> RythmicGroups { get; } = new();
        public void MovePointerToRG(long rythmicGroupIndex)
        {
            PointerPosition = (rythmicGroupIndex * 135) + 35;
        }
    }
}
