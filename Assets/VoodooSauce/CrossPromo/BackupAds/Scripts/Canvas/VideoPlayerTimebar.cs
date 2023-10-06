using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Voodoo.Sauce.Internal.CrossPromo.BackupAds.Utils
{
    public class VideoPlayerTimebar : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public Image fillImage;

        private void Update()
        {
            if (!videoPlayer) return;

            double elapsed = videoPlayer.time;
            double duration = videoPlayer.length;
            double percent = elapsed / duration;

            fillImage.fillAmount = Mathf.Clamp01((float)percent);
        }
    }
}
