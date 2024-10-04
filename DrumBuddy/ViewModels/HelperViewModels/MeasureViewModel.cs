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
            //ha 0: 35
            //ha 1: 35+135=170
            //ha 2: 170+135=305
            //ha 3: 305+135=440
            PointerPosition = (indexOfCurrentRythmicGroup * 135) + 35;
            //PointerPosition = (indexOfCurrentRythmicGroup * 150) + 25;
        }
    }
}
