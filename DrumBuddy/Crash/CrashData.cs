using System;

namespace DrumBuddy.Crash;

public record CrashData(DateTimeOffset CrashDate, string Source, string ErrorMessage, string StackTrace);