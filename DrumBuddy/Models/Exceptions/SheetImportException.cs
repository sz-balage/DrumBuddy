using System;

namespace DrumBuddy.Models.Exceptions;

public class SheetImportException(string Message, string SheetName) : Exception(Message)
{
    public string SheetName { get; } = SheetName;
}