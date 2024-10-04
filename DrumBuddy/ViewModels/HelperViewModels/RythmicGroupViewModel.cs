using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.HelperViewModels
{
    public partial class RythmicGroupViewModel : ReactiveObject
    {
        // Add properties and logic for RythmicGroup if needed
        [Reactive]
        public RythmicGroup _rythmicGroup;
        public void AddNotes(IList<Note> notes)
        {
            RythmicGroup = new(notes.ToImmutableArray());
            //draw
        }
    }
}
