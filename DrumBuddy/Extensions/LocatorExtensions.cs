using System;
using Splat;

namespace DrumBuddy.Extensions;

public static class LocatorExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver provider)
    {
        var service = provider.GetService<T>();
        if (service is null) throw new Exception($"Service of type {typeof(T).FullName} could not be resolved.");
        return service;
    }

    public static object GetRequiredService(this IReadonlyDependencyResolver provider, Type serviceType)
    {
        var service = provider.GetService(serviceType);
        if (service is null) throw new Exception($"Service of type {serviceType.FullName} could not be resolved.");
        return service;
    }
}