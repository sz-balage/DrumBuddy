using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DrumBuddy.Client.Models;

public class NavigationMenuItemTemplate
{
    public NavigationMenuItemTemplate(Type modelType, string iconKey, string description)
    {
        ModelType = modelType;
        Label = modelType.Name.Replace("ViewModel", "");
        Description = description;
        Icon = (StreamGeometry)Application.Current!.FindResource(iconKey);
    }

    public Type ModelType { get; }
    public string Label { get; }
    public string Description { get; }
    public StreamGeometry Icon { get; }
}