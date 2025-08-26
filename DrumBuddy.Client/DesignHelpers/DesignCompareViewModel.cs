using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DrumBuddy.Client.ViewModels.Dialogs;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using ReactiveUI;

namespace DrumBuddy.Client.DesignHelpers;

public class DesignCompareViewModel : ReactiveObject, ICompareViewModel
{
    public ReadOnlyObservableCollection<MeasureViewModel> BaseSheetMeasures { get; }
    public ReadOnlyObservableCollection<MeasureViewModel> ComparedSheetMeasures { get; }
    public string BaseSheetName { get; } = "A";
    public string ComparedSheetName { get; } = "B";

    public DesignCompareViewModel()
    {
        var baseMeasures =  new List<Measure>
        {
            new Measure([
                new RythmicGroup([
                    new NoteGroup([
                        new Note(Drum.Crash1, NoteValue.Eighth)
                    ])
                ])
            ]),
            new Measure([
                new RythmicGroup([
                    new NoteGroup([
                        new Note(Drum.Crash1, NoteValue.Quarter)
                    ])
                ])
            ])
        }; 
        var comparedMeasures = new List<Measure>
        {
            new Measure([
                new RythmicGroup([
                    new NoteGroup([
                        new Note(Drum.Crash1, NoteValue.Eighth)
                    ])
                ])
            ]),
            new Measure([
                new RythmicGroup([
                    new NoteGroup([
                        new Note(Drum.Crash1, NoteValue.Quarter)
                    ])
                ])
            ])
        };

        var baseVm = new ObservableCollection<MeasureViewModel>(baseMeasures.Select(m => new MeasureViewModel(m)));
        var compareVm = new ObservableCollection<MeasureViewModel>(comparedMeasures.Select(m => new MeasureViewModel(m)));

        BaseSheetMeasures = new ReadOnlyObservableCollection<MeasureViewModel>(baseVm);
        ComparedSheetMeasures = new ReadOnlyObservableCollection<MeasureViewModel>(compareVm);
    }
}