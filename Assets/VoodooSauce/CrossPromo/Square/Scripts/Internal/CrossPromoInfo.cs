using System;
using System.Collections.Generic;
using UnityEngine;
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Internal.CrossPromo.Utils;
using Random = System.Random;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.CrossPromo
{
    /// <summary>
    /// Store all the information about the cross promotion
    /// </summary>
    internal class CrossPromoInfo
    {
        public bool HasInternet;

        public CrossPromoInfo()
        {
            CurrentGame = null;
            Assets = new List<AssetModel>();
            Waterfall = new List<AssetModel>();
            GamesInstalled = new List<AssetModel>();
        }

        public GameModel CurrentGame { get; set; }
        
        /// <summary>
        /// Unique identifier of the current cross promo
        /// </summary>
        public string Uuid { private set; get; }

        public void GenerateUuid() => Uuid = Guid.NewGuid().ToString();

        /// <summary>
        /// Filtered assets to display
        /// </summary>
        public List<AssetModel> Assets { get; }

        public List<AssetModel> Waterfall { get; }

        public List<AssetModel> GamesInstalled { get; }

        public bool CrossPromoIsReady { get; set; }

        /// <summary>
        /// Chosen format for the cross promotion
        /// </summary>
        public string Format { get; set; }

        public bool GetInternetStatus() =>
            !HasInternet || Application.internetReachability == NetworkReachability.NotReachable;

        /// <summary>
        /// Filter the assets if the user has already installed a game
        /// </summary>
        public void FilterAssets()
        {
            if (CurrentGame == null) return;

            Assets.Clear();
            foreach (AssetModel a in CurrentGame.promote_assets) {
                if (MobileCrossPromoWrapper.IsAppInstalled(a.game))
                    GamesInstalled.Add(a);
                else
                    Assets.Add(a);
            }
        }

        public void FillAssetsListFromCache()
        {
            Assets.Clear();
            foreach (string assetString in CacheManager.GetAllFilesFromCache()) {
                try {
                    string assetJson =
                        PlayerPrefs.GetString(PlayerPrefsUtils.GetKey(assetString));
                    if (string.IsNullOrEmpty(assetJson))
                        throw new Exception();
                    var asset = JsonUtility.FromJson<AssetModel>(assetJson);
                    Assets.Add(asset);
                } catch (Exception) {
                    CacheManager.DeleteFile(assetString);
                }
            }

            Shuffle(Assets);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}