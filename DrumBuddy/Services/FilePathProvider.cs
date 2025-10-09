using System;
using System.IO;

namespace DrumBuddy.Services;

public class FilePathProvider
{
    public static string GetPathToHighBeepSound()
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", "metronomeup.wav");
    }

    public static string GetPathToRegularBeepSound()
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", "metronome.wav");
    }

    public static string GetPathForSavedFiles()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "SavedFiles");
    }

    public static string GetPathForCrashData()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "ErrorLogs");
    }
}