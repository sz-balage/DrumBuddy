using System.Collections.ObjectModel;
using DrumBuddy.Core.Enums;
using ReactiveUI;

namespace DrumBuddy.ViewModels.HelperViewModels;

public class RowViewModel : ReactiveObject
{
    public RowViewModel(Drum drum)
    {
        Drum = drum;
        Cells = new ObservableCollection<StepCellViewModel>();
    }

    public Drum Drum { get; }
    public string Name => Drum.ToString();
    public ObservableCollection<StepCellViewModel> Cells { get; }
}