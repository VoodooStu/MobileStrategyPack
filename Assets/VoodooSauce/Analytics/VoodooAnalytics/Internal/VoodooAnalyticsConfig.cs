using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Analytics
{
    public static class VoodooAnalyticsConfig
    { 
        [CanBeNull] public static AnalyticsConfig AnalyticsConfig { get; set; }
    }
}