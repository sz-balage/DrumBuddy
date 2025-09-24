using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DrumBuddy.Core.Models;
using DrumBuddy.Models;
using DrumBuddy.ViewModels.HelperViewModels;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class CompareViewModel : ReactiveObject, ICompareViewModel
{
    private readonly Sheet _baseSheet;

    private readonly ReadOnlyObservableCollection<MeasureViewModel> _baseSheetMeasures;
    private readonly SourceList<MeasureViewModel> _baseSheetMeasureSource = new();
    private readonly Sheet _comparedSheet;

    private readonly ReadOnlyObservableCollection<MeasureViewModel> _comparedSheetMeasures;
    private readonly SourceList<MeasureViewModel> _comparedSheetMeasureSource = new();

    [Reactive] public double _correctPercentage;

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
        EvaluateSheets();
        for (int i = 0; i < _comparedSheetMeasures.Count; i++)
        {
            _comparedSheetMeasures[i].EvaluateMeasure(EvaluationBoxes.Where(x => x.MeasureIndex == i).ToList());
        }
    }

    private List<EvaluationBox> EvaluationBoxes { get; } = new();
    public ReadOnlyObservableCollection<MeasureViewModel> BaseSheetMeasures => _baseSheetMeasures;
    public ReadOnlyObservableCollection<MeasureViewModel> ComparedSheetMeasures => _comparedSheetMeasures;
    public string BaseSheetName => _baseSheet.Name;
    public string ComparedSheetName => _comparedSheet.Name;

    private void EvaluateSheets()
    {
        EvaluationBoxes.Clear();

        int measureCount = Math.Min(_baseSheet.Measures.Length, _comparedSheet.Measures.Length);

        int totalGroups = 0;
        int correctGroups = 0;

        for (int m = 0; m < measureCount; m++)
        {
            var baseMeasure = _baseSheet.Measures[m];
            var comparedMeasure = _comparedSheet.Measures[m];

            int rgCount = Math.Min(baseMeasure.Groups.Count, comparedMeasure.Groups.Count);

            totalGroups += rgCount;

            EvaluationState? currentState = null;
            int startRg = 0;

            for (int rg = 0; rg < rgCount; rg++)
            {
                var same = baseMeasure.Groups[rg].Equals(comparedMeasure.Groups[rg]);
                var state = same ? EvaluationState.Correct : EvaluationState.Incorrect;

                if (same) correctGroups++;

                if (currentState == null)
                {
                    startRg = rg;
                    currentState = state;
                }
                else if (state != currentState)
                {
                    EvaluationBoxes.Add(new EvaluationBox(m, startRg, rg - 1, currentState.Value));
                    startRg = rg;
                    currentState = state;
                }
            }

            if (currentState != null)
            {
                EvaluationBoxes.Add(new EvaluationBox(m, startRg, rgCount - 1, currentState.Value));
            }
        }

        if (_baseSheet.Measures.Length > _comparedSheet.Measures.Length)
        {
            int extraMeasures = _baseSheet.Measures.Length - _comparedSheet.Measures.Length;
            totalGroups += extraMeasures * 4;
        }

        CorrectPercentage = totalGroups == 0
            ? 0
            : (double)correctGroups / totalGroups * 100.0;
    }
}

public interface ICompareViewModel
{
    ReadOnlyObservableCollection<MeasureViewModel> BaseSheetMeasures { get; }
    ReadOnlyObservableCollection<MeasureViewModel> ComparedSheetMeasures { get; }
    public string BaseSheetName { get; }
    public string ComparedSheetName { get; }
    public double CorrectPercentage { get; }
}