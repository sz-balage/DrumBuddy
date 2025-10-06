using ReactiveUI;

namespace DrumBuddy.ViewModels.Dialogs;

public class ConfirmationViewModel : ReactiveObject
{
    private string _message = "Are you sure?";

    private bool _showDiscard = true;

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public bool ShowDiscard
    {
        get => _showDiscard;
        set => this.RaiseAndSetIfChanged(ref _showDiscard, value);
    }

    public string ConfirmText { get; set; } = "Save";
    public string DiscardText { get; set; } = "Discard";
    public string CancelText { get; set; } = "Cancel";
}