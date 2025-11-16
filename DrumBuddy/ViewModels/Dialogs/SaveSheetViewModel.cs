using System;
using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.Extensions;
using DrumBuddy.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Splat;

namespace DrumBuddy.ViewModels.Dialogs;

public partial class SaveSheetViewModel : ReactiveObject, IValidatableViewModel
{
    private readonly LibraryViewModel _library;
    private readonly SheetCreationData _sheetCreationData;
    [Reactive] private string _sheetDescription = "";
    [Reactive] private string _sheetName = "";
    private Guid? _sheetId;
    public SaveSheetViewModel(SheetCreationData sheetCreationData, Guid? sheetId = null)
    {
        _sheetId = sheetId;
        _library = Locator.Current.GetRequiredService<LibraryViewModel>();
        _sheetCreationData = sheetCreationData;
        var titleObservable =
            this.WhenAnyValue(vm => vm.SheetName);
        this.ValidationRule(
            viewModel => viewModel.SheetName,
            titleObservable,
            name => !string.IsNullOrEmpty(name) && !_library.SheetExists(name.Trim()),
            n => string.IsNullOrEmpty(n)
                ? "Sheet title cannot be empty!"
                : "Sheet with this name already exists!");
    }

    private IObservable<bool> _saveSheetCanExecute => this.IsValid();

    public IValidationContext ValidationContext { get; } = new ValidationContext();

    [ReactiveCommand(CanExecute = nameof(_saveSheetCanExecute))]
    private async Task SaveSheet()
    {
        _sheetName = _sheetName.Trim();
        Sheet sheetToSave = new(_sheetCreationData.Bpm, _sheetCreationData.Measures, _sheetName, _sheetDescription,_sheetId);
        await _library.SaveSheet(sheetToSave);
    }
}