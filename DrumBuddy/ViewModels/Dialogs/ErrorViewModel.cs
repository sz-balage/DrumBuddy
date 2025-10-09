using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class ErrorViewModel : ReactiveObject
{
    [Reactive] private string _description = "Unknown Error Description";
    [Reactive] private string _title = "Unknown Error";
}