using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DrumBuddy.Client.Extensions;
using DrumBuddy.Client.Models;
using DrumBuddy.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Splat;

namespace DrumBuddy.Client.ViewModels.Dialogs;

public partial class SaveSheetViewModel : ReactiveObject, IValidatableViewModel
{
    private IObservable<bool> _saveSheetCanExecute => this.IsValid();
    private readonly LibraryViewModel _library;
    private readonly SheetCreationData _sheetCreationData;
    [Reactive] private string _sheetName;
    [Reactive] private string _sheetDescription = "";

    public SaveSheetViewModel(SheetCreationData sheetCreationData)
    {
        _library = Locator.Current.GetRequiredService<LibraryViewModel>();
        _sheetCreationData = sheetCreationData;
        var titleObservable =
            this.WhenAnyValue(
                vm => vm.SheetName);
        this.ValidationRule(
            viewModel => viewModel.SheetName,
            titleObservable,
            name =>!string.IsNullOrEmpty(name) && !_library.SheetExists(name.Trim()),
            n =>
            {
                return string.IsNullOrEmpty(n) ? "Sheet title cannot be empty!" : "Sheet with this name already exists!";
            });
    }
    [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
    private async Task SaveSheet()
    {
        _sheetName = _sheetName.Trim();
        Sheet sheetToSave = new(_sheetCreationData.Bpm, _sheetCreationData.Measures, _sheetName, _sheetDescription);
        await _library.SaveSheet(sheetToSave);
    }
    public IValidationContext ValidationContext { get; } = new ValidationContext();
}