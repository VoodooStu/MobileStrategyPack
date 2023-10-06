using System;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    [Serializable]
    public class AudioAdConfig
    {
        public string adNetwork;
        public int gameStartTriggerFrequency;
        public int coolDownBetweenAudioAds;
        public bool killWhenFsOrRvStarts;
        public bool killWhenGameFinishes;
    }
}