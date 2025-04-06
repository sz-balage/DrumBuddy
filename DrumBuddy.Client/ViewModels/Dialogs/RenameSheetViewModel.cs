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
    public Sheet OriginalSheet { get; }
    [Reactive]
    private string _newName;
    [Reactive]
    private string _newDescription;

    public RenameSheetViewModel(Sheet sheet)
    {
        var library = Locator.Current.GetService<LibraryViewModel>()!;
        OriginalSheet = sheet; 
        this.ValidationRule(
            viewModel => viewModel.NewName,
            name => !string.IsNullOrEmpty(name) && (name == OriginalSheet.Name || !library.SheetExists(name)),
            n => string.IsNullOrEmpty(n) ? "Sheet title cannot be empty!" : "Sheet with this name already exists!");
        NewName = sheet.Name;
        NewDescription = sheet.Description;
    }

    [ReactiveCommand(CanExecute = nameof(RenameCanExecute))]
    public void RenameSheet()
    { }
}