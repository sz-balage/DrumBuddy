using System;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Splat;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public partial class RenameSheetViewModel : ReactiveObject, IValidatableViewModel
{
    private IObservable<bool> RenameCanExecute => this.IsValid();
    public IValidationContext ValidationContext { get; } = new ValidationContext();
    public string OriginalName { get; }
    [Reactive]
    private string _newName;
    private LibraryViewModel _library;
    public RenameSheetViewModel(Sheet sheet)
    {
        _library = Locator.Current.GetService<LibraryViewModel>()!;
        OriginalName = sheet.Name; 
        this.ValidationRule(
            viewModel => viewModel.NewName,
            name => !string.IsNullOrEmpty(name) && !_library.SheetExists(name),
            n => string.IsNullOrEmpty(n) ? "Sheet title cannot be empty!" : "Sheet with this name already exists!");
    }

    [ReactiveCommand(CanExecute = nameof(RenameCanExecute))]
    public void RenameSheet()
    { }
}