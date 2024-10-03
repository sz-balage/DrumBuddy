using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using DrumBuddy.Core.Models;
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

        public void MovePointerToNextRythmicGroup(int indexOfCurrentRythmicGroup)
        {
            PointerPosition = indexOfCurrentRythmicGroup * 125;
        }
    }
}
