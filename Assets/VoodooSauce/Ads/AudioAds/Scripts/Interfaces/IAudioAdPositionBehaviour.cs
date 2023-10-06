using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public interface IAudioAdPositionBehaviour
    {
        public Canvas Canvas { get; }
        public RectTransform RectTransform { get; }
        public bool IsActiveAndEnabled { get; }
    }
}