using System.Collections.Generic;
using System.Collections.Immutable;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using DrumBuddy.Core.Models;
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
        public ObservableCollection<RythmicGroupViewModel> RythmicGroups { get; } = new()
        {
            new RythmicGroupViewModel(),
            new RythmicGroupViewModel(),
            new RythmicGroupViewModel(),
            new RythmicGroupViewModel()
        };

        public void AddNotesToRythmicGroup((IList<Note> notes, int rythmicGroupIndex) tuple)
        {
            Measure.Groups[tuple.rythmicGroupIndex] = new RythmicGroup(tuple.notes.ToImmutableArray());
            PointerPosition = (tuple.rythmicGroupIndex * 135) + 35;
            RythmicGroups[tuple.rythmicGroupIndex].AddNotes(tuple.notes);
        }
        public bool IsEmpty => RythmicGroups.All(rg => rg.RythmicGroup.IsDefault());
    }
}
