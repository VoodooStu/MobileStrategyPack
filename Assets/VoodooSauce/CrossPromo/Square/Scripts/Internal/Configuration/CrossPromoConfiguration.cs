using System;

namespace Voodoo.Sauce.Internal.CrossPromo.Configuration
{
    /// <summary>
    /// Class used to remotely configure cross promotion from VoodooTune
    /// Technical name: Voodoo.Sauce.Internal.CrossPromo.Configuration.CrossPromoConfiguration
    /// </summary>
    [Serializable]
    internal class CrossPromoConfiguration
    {
        /// <summary>
        /// If false, the cross promotion will not be enabled
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// Number of sessions required for a new user to have the cross promotion displayed for the first time
        /// 0 equals always
        /// </summary>
        public int sessionsCountBeforeShowing = 0;
        
        /// <summary>
        /// Number of sessions required since the last session where the cross promotion was displayed
        /// 0 equals always
        /// </summary>
        public int sessionsCountBetweenShowing = 0;
        
        /// <summary>
        /// Number of games required in the current session to have the cross promotion displayed for the first time
        /// 0 equals always
        /// </summary>
        public int gamesCountBeforeShowing = 0;
        
        /// <summary>
        /// Number of games required since the last game index where the cross promotion was displayed
        /// 0 equals always
        /// </summary>
        public int gamesCountBetweenShowing = 0;
        
        /// <summary>
        /// Number of impressions (without any user click/interaction) before hiding the cross promo in the current session (Resets in the next session)
        /// -1 equals never hides
        /// </summary>
        public int impressionsCountBeforeHiding = -1;

        /// <summary>
        /// List of the bundle id of the games to show separated by a ',' comma
        /// </summary>
        public string gamesToShow = "";

        /// <summary>
        /// Id of the Waterfall. If empty, the cohort_uuid will be used instead.
        /// </summary>
        public string waterfallId = "";

        public override string ToString()
        {
            string str = "CrossPromoConfiguration:" + Environment.NewLine + Environment.NewLine;

            str += $"enabled: {enabled}" + Environment.NewLine;
            
            str += $"sessionsCountBeforeShowing: {sessionsCountBeforeShowing}" + Environment.NewLine;
            str += $"sessionsCountBetweenShowing: {sessionsCountBetweenShowing}" + Environment.NewLine;
            
            str += $"gamesCountBeforeShowing: {gamesCountBeforeShowing}" + Environment.NewLine;
            str += $"gamesCountBetweenShowing: {gamesCountBetweenShowing}" + Environment.NewLine;
            
            str += $"impressionsCountBeforeHiding: {impressionsCountBeforeHiding}" + Environment.NewLine;

            str += $"waterfallId: {waterfallId}" + Environment.NewLine;
            str += $"gamesToShow: '{gamesToShow}'";
            
            return str;
        }
    }
}