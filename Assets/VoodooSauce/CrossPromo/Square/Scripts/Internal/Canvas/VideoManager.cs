using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Voodoo.Sauce.Internal.CrossPromo.Mobile;
using Voodoo.Sauce.Internal.CrossPromo.Models;
using Voodoo.Sauce.Internal.CrossPromo.Utils;

namespace Voodoo.Sauce.Internal.CrossPromo.Canvas
{
    public class VideoManager
    {
        public RawImage Content { get; }

        public VideoPlayer VideoPlayer { get; }

        private readonly RenderTexture _renderTexture;

        public VideoManager(VideoPlayer video, RawImage content)
        {
            VideoPlayer = video;
            Content = content;

            Content.enabled = false;

            VideoPlayer.prepareCompleted += OnVideoComplete;

            _renderTexture = new RenderTexture(0, 0, 24);
        }

        public static AssetModel ChooseVideo()
        {
            CrossPromoInfo infos = VoodooCrossPromo.Info;
            if (infos.Assets.Count <= infos.Waterfall.Count)
                infos.Waterfall.Clear();

            var assets = new List<AssetModel>(infos.Assets);
            foreach (AssetModel asset in assets) {
                if (!CacheManager.IsFileExist(asset.file_path) || infos.Waterfall.Contains(asset)) continue;
                if (MobileCrossPromoWrapper.IsAppInstalled(asset.game)) {
                    infos.Assets.Remove(asset);
                    continue;
                }

                infos.Waterfall.Add(asset);
                return asset;
            }

            infos.Waterfall.Clear();
            if (infos.Assets.Count == 0) return null;
            AssetModel firstElement = infos.Assets[0];
            infos.Waterfall.Add(firstElement);
            return firstElement;
        }

        public void PrepareVideo(AssetModel asset)
        {
            if (_renderTexture.IsCreated())
                ReleaseTexture();
            if (!CacheManager.IsFileExist(asset.file_path))
                throw new Exception("Can not read the file because it doesn't exist");
            VideoPlayer.url = CacheManager.Path + asset.file_path;
            VideoPlayer.Prepare();
        }

        public void StopVideo()
        {
            Content.enabled = false;
            VideoPlayer.Stop();
        }

        private void ReleaseTexture()
        {
            _renderTexture.Release();
            _renderTexture.DiscardContents();
        }

        /*
         * EVENTS OF THE VIDEO PLAYER
         */

        private void OnVideoComplete(VideoPlayer source)
        {
            Texture texture = source.texture;
            int width = texture.width;
            int height = texture.height;

            if (!_renderTexture.IsCreated()) {
                _renderTexture.width = width;
                _renderTexture.height = height;
            }

            _renderTexture.Create();

            VideoPlayer.targetTexture = _renderTexture;
            Content.texture = _renderTexture;
            Content.enabled = true;

            ReleaseTexture();
        }
    }
}