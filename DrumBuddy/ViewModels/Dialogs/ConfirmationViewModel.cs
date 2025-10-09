using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class ConfirmationViewModel : ReactiveObject
{
    [Reactive] private string _message = "Are you sure?";

    [Reactive] private bool _showConfirm = true;

    [Reactive] private bool _showDiscard = true;

    public string ConfirmText { get; set; } = "Save";
    public string DiscardText { get; set; } = "Discard";
    public string CancelText { get; set; } = "Cancel";
}