using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Sauce.Internal.CrossPromo.Mercury.Models
{
    [Serializable]
    public class MercuryWaterfall
    {
        public string id;
        public string name;
        public MercuryPromotedAsset[] promote_assets;
        public int first_time_videos_in_cache;
        public int buffer_videos;
        public string os_type;
        public string strategy_id;

        public string GetPromotedAssets()
        {
            var names = new List<string>();
            for (var index = 0; index < promote_assets.Length; index++) {
                MercuryPromotedAsset asset = promote_assets[index];
                string formatter = $"{index + 1}. {asset.game.name}"; 
                names.Add(formatter);
            }

            return string.Join(", ", names);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}