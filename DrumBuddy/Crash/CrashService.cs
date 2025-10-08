using System;

namespace DrumBuddy.Crash;

public static class CrashService
{
    public static CrashData GetCrashData(Exception ex)
    {
        return new CrashData(
            DateTimeOffset.UtcNow,
            ErrorMessage: ex.Message,
            StackTrace: ex.StackTrace ?? string.Empty,
            Source: ex.TargetSite?.ToString() ?? string.Empty);
    }
}