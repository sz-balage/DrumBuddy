using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DrumBuddy.Client.ViewModels.HelperViewModels;
using DrumBuddy.Core.Models;
using DynamicData;
using ReactiveUI;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public class CompareViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<MeasureViewModel> _baseSheetMeasures;
    private readonly SourceList<MeasureViewModel> _baseSheetMeasureSource = new();
    public ReadOnlyObservableCollection<MeasureViewModel> BaseSheetMeasures => _baseSheetMeasures;

    private readonly ReadOnlyObservableCollection<MeasureViewModel> _comparedSheetMeasures;
    private readonly SourceList<MeasureViewModel> _comparedSheetMeasureSource = new();
    public ReadOnlyObservableCollection<MeasureViewModel> ComparedSheetMeasures => _comparedSheetMeasures;
    private readonly Sheet _baseSheet;
    private readonly Sheet  _comparedSheet;

    public CompareViewModel((Sheet baseSheet, Sheet comparedSheet) argInput)
    {
        _baseSheet = argInput.baseSheet;
        _comparedSheet = argInput.comparedSheet;
        _baseSheetMeasureSource.Connect()
            .Bind(out _baseSheetMeasures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _comparedSheetMeasureSource.Connect()
            .Bind(out _comparedSheetMeasures)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe();
        _baseSheetMeasureSource.AddRange(_baseSheet.Measures.Select(m => new MeasureViewModel(m)));
        _comparedSheetMeasureSource.AddRange(_comparedSheet.Measures.Select(m => new MeasureViewModel(m)));
        
    }
}