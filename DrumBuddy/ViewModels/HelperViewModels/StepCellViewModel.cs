using System;
using System.Reactive;
using ReactiveUI;

namespace DrumBuddy.ViewModels.HelperViewModels;

public class StepCellViewModel : ReactiveObject
{
    private bool _isChecked;
    private bool _isEnabled = true;

    public StepCellViewModel(int row, int column, Func<int, int, Unit> toggleCallback)
    {
        Row = row;
        Column = column;
        ToggleCommand = ReactiveCommand.Create(() =>
        {
            toggleCallback?.Invoke(Row, Column);
            return Unit.Default;
        });
    }

    public int Row { get; }
    public int Column { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleCommand { get; }
}