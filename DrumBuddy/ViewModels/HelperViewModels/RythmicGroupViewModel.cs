using Avalonia;
using Avalonia.Media;
using DrumBuddy.Core.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.HelperViewModels
{
    public partial class RythmicGroupViewModel : ReactiveObject
    {
        // Add properties and logic for RythmicGroup if needed
        public RythmicGroupViewModel(RythmicGroup rg)
        {
            this.RythmicGroup = rg;
            this.Drawing = new EllipseGeometry()
            {
                Center = new Point(30, 30),
                RadiusX = 30,
                RadiusY = 30
            };
            this.Color = rg.Notes.Length == 0 ? Brushes.Red : Brushes.Green;
        }
        [Reactive]
        private RythmicGroup _rythmicGroup;
        [Reactive]
        private EllipseGeometry _drawing;

        [Reactive] private IImmutableBrush _color;
    }
}
