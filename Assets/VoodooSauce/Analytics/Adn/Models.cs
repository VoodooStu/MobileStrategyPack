using System.Collections.Generic;
using Voodoo.Sauce.Internal.Extension;
using AdnConstants = Voodoo.Sauce.Internal.Analytics.AdnAnalyticsConstants;

namespace Voodoo.Sauce.Internal.Analytics
{
    internal class AdSession
    {
        public string Placement;
        public string AdNetwork;
        public double? Cpm;
        public string CreativeId;
        public bool IsClicked;
        public int Duration;
        public bool IsSkipped;

        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object> {
                {AdnConstants.PLACEMENT, Placement},
                {AdnConstants.DURATION, Duration},
                {AdnConstants.IS_CLICKED, IsClicked ? 1 : 0},
                {AdnConstants.IS_SKIPPED, IsSkipped ? 1 : 0}
            };
            dictionary.AddIfNotNull(AdnConstants.CREA_ID, CreativeId);
            dictionary.AddIfNotNull(AdnConstants.AD_NETWORK, AdNetwork);
            dictionary.AddIfNotNull(AdnConstants.CPM, Cpm);
            return dictionary;
        }
    }

    internal class AdRevenue
    {
        public double FsRevenue;
        public double RvRevenue;

        public Dictionary<string, object> ToDictionary() => new Dictionary<string, object> {
            {AdnConstants.FS, FsRevenue},
            {AdnConstants.RV, RvRevenue}
        };
    }

    internal class AdClick
    {
        public bool IsRvClicked;
        public bool IsFsClicked;

        public Dictionary<string, object> ToDictionary() => new Dictionary<string, object> {
            {AdnConstants.FS, IsFsClicked ? 1 : 0},
            {AdnConstants.RV, IsRvClicked ? 1 : 0}
        };
    }

    internal class PerfMetrics
    {
        public int BadFrames;
        public int TerribleFrames;

        public Dictionary<string, object> ToDictionary() => new Dictionary<string, object> {
            {AdnConstants.BAD_FRAMES, BadFrames},
            {AdnConstants.TERRIBLE_FRAMES, TerribleFrames}
        };
    }
}