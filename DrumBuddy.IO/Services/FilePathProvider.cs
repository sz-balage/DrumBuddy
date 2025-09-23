namespace DrumBuddy.IO.Services;

public static class FilePathProvider //TODO: should be part of client, not IO, and use assetloader there
{
    public static string GetPathToHighBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets", "metronomeup.wav");
    }

    public static string GetPathToRegularBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets", "metronome.wav");
    }

    public static string GetPathForFileStorage()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "DrumBuddy", "SavedFiles");
    }
}