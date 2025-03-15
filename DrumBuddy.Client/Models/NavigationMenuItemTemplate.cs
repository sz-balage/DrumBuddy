using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DrumBuddy.Client.Models;

public class NavigationMenuItemTemplate
{
    public NavigationMenuItemTemplate(Type modelType, string iconKey)
    {
        ModelType = modelType;
        Label = modelType.Name.Replace("ViewModel", "");
        Icon = (StreamGeometry)Application.Current!.FindResource(iconKey);
    }

    public Type ModelType { get; }
    public string Label { get; }
    public StreamGeometry Icon { get; }
}