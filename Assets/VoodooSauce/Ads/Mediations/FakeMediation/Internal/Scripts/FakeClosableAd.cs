using System;
using UnityEditor;

namespace Voodoo.Sauce.Internal.Ads.FakeMediation
{
    internal abstract class FakeClosableAd : FakeAd
    {
        public Action onClose;

        protected void Close()
        {
            onClose?.Invoke();
            StopAd();
        }
    }
}