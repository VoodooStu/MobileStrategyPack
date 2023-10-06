// ReSharper disable once CheckNamespace
using System;

namespace Voodoo.Sauce.Ads
{
    public enum NativeAdLayout
    {
        Square,
        Rectangle
    }

    internal static class NativeAdLayoutExtension
    {
        internal static string GetKey(this NativeAdLayout layout)
        {
            switch (layout) {
                case NativeAdLayout.Square:
                    return "NativeAdSquareView";
                case NativeAdLayout.Rectangle:
                    return "NativeAdRectangleView";
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout), layout, null);
            }
        }

        internal static float GetRatio(this NativeAdLayout layout)
        {
            switch (layout)
            {
                case NativeAdLayout.Square:
                    return .87f;
                case NativeAdLayout.Rectangle:
                    return 2.3f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout), layout, null);
            }
        }
    }
}