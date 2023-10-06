using Voodoo.Sauce.Internal.Ads;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Core
{
    /// <summary>
    /// This interface is used to mock our VoodooSettings. It's not complete yet.
    /// Feel free to complete it while adding new tests.
    /// </summary>
    public interface IVoodooSettings
    {
        public AudioAdConfig GetIosAudioAdConfig { get; }
        public AudioAdConfig GetAndroidAudioAdConfig { get; }
    }
}