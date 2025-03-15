using System;
using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;

namespace DrumBuddy.Client.Models;

public class NoteImageAndBounds : ReactiveObject
{
    private static readonly ConcurrentDictionary<Uri, Bitmap> ImageCache = new();

    public Uri ImagePath { get; }
    public Rect Bounds { get; }
    public Bitmap Image { get; }

    public NoteImageAndBounds(Uri imagePath, Rect bounds)
    {
        ImagePath = imagePath;
        Bounds = bounds;
        Image = GetOrCreateImage(imagePath);
    }

    public NoteImageAndBounds(Bitmap image, Rect bounds) //for testing purposes
    {
        Image = image;
        Bounds = bounds;
    }

    private static Bitmap GetOrCreateImage(Uri imagePath)
    {
        return ImageCache.GetOrAdd(imagePath, path =>
        {
            try
            {
                return new Bitmap(AssetLoader.Open(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {path}. Error: {ex.Message}");
                throw;
            }
        });
    }

    // Add a method to clear the cache if needed (e.g., when freeing resources)
    public static void ClearCache()
    {
        ImageCache.Clear();
    }
}