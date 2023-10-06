using System.Collections.Generic;
using System.Globalization;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.Utils;
using Voodoo.Tune.Core;

namespace Voodoo.Sauce.Internal.VoodooTune
{
	public class VoodooTuneSessionParameters : IVoodooTuneFilledVariables
	{
        private enum SessionParameter
        {
            app_open_count,
            vs_version,
            advertising_id,
            sessions_count,
            games_count,
            best_score,
            level,
            win_rate,
            user_id,
            device_model,
            device_manufacturer,
            developer_device_id
        }
        
        public Dictionary<string,string> GetSessionParameters()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
           
            Dictionary<SessionParameter, string> sessionParameters = BuildSessionParameters();
            foreach (var pair in sessionParameters)
            {
                res.Add(pair.Key.ToString(), pair.Value);
            }

            return res;
        }
       
        private Dictionary<SessionParameter, string> BuildSessionParameters()
        {
            var parameters = new Dictionary<SessionParameter, string>
            {
                {
                    SessionParameter.vs_version,
                    VoodooSauce.Version()
                },
                {
                    SessionParameter.app_open_count,
                    AnalyticsStorageHelper.Instance.GetAppLaunchCount().ToString()
                },
                {
                    SessionParameter.sessions_count,
                    AnalyticsSessionManager.Instance().SessionInfo.count.ToString()
                },
                {
                    SessionParameter.games_count,
                    AnalyticsStorageHelper.Instance.GetGameCount().ToString()
                },
                {
                    SessionParameter.user_id,
                    AnalyticsUserIdHelper.GetUserId()
                },
                {
                    SessionParameter.device_model,
                    DeviceUtils.Model
                },
                {
                    SessionParameter.device_manufacturer,
                    DeviceUtils.Manufacturer
                }
            };

            var privacy = VoodooSauceCore.GetPrivacy();
            if (privacy.HasLimitAdTrackingEnabled() == false)
            {
                parameters.Add(SessionParameter.advertising_id, privacy.GetAdvertisingId(false));
            }
            
            if (string.IsNullOrEmpty(privacy.GetVendorId()) == false)
            {
                parameters.Add(SessionParameter.developer_device_id, privacy.GetVendorId());
            }

            if (AnalyticsStorageHelper.Instance.HasWinRate())
            {
                var sessionParameter = SessionParameter.win_rate;
                var winRate = AnalyticsStorageHelper.Instance.GetWinRate().ToString("0.##", CultureInfo.CreateSpecificCulture("en-US"));

                parameters.Add(sessionParameter, winRate);
            }

            if (AnalyticsStorageHelper.Instance.HasGameHighestScore())
            {
                var sessionParameter = SessionParameter.best_score;
                var bestScore = AnalyticsStorageHelper.Instance.GetGameHighestScore().ToString("0.##", CultureInfo.CreateSpecificCulture("en-US"));

                parameters.Add(sessionParameter, bestScore);
            }

            if (AnalyticsStorageHelper.Instance.HasCurrentLevel())
            {
                var sessionParameter = SessionParameter.level;
                var level = AnalyticsStorageHelper.Instance.GetCurrentLevel();

                parameters.Add(sessionParameter, level);
            }

            return parameters;
        }
	}
}