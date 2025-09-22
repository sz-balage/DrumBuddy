namespace DrumBuddy.IO.Services;

public static class FileSystemService //TODO: should be part of client, not IO, and use assetloader there
{
    public static string GetPathToHighBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets","metronomeup.wav");
    }

    public static string GetPathToRegularBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets","metronome.wav");
    }
}