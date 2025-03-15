namespace DrumBuddy.IO.Services;

public static class FileSystemService
{
    public static string GetPathToHighBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets\\metronomeup.wav");
    }

    public static string GetPathToRegularBeepSound()
    {
        var dir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;
        return dir is null
            ? ""
            : Path.Combine(dir.FullName, "Assets\\metronome.wav");
    }
}