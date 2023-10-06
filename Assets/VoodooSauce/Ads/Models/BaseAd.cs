using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Ads
{
    public abstract class BaseAd
    {
        protected readonly IMediationAdapter MediationAdapter;
        protected readonly string AppVersion;
        
        public AdLoadingState State { get; protected set; }
        
        // There are two cached unique identifiers, because a second ad could be loaded while the first one is displayed:
        // - Uuid is saved as soon as an ad is loaded.
        // - UuidBeingShown is used while the ad is displayed.
        protected string Uuid { get; private set; }
        
        protected string UuidBeingShown { get; private set; }
        
        public AdTimer LoadingTime { get; }
        public AdDisplayConditions AdDisplayConditions;
        
        protected string Type;
        
        protected BaseAd(IMediationAdapter adapter)
        {
            MediationAdapter = adapter;
            State = AdLoadingState.NotInitialized;
            LoadingTime = new AdTimer();
            AppVersion = Application.version;
        } 
        
        internal abstract void Initialize();
        
        internal bool IsAvailable() => State == AdLoadingState.Loaded;
        
        internal bool IsEnabled() => State != AdLoadingState.Disabled;

        public virtual void Enable() => State = AdLoadingState.NotInitialized;

        public virtual void Disable() => State = AdLoadingState.Disabled;

        internal int LoadingTimeMilliseconds() => (int) LoadingTime.TotalMilliseconds;

        protected string GenerateUuid()
        {
            Uuid = Guid.NewGuid().ToString();
            return Uuid;
        }

        protected virtual void StartShowing()
        {
            UuidBeingShown = Uuid;
        }

        protected virtual void StopShowing()
        {
            UuidBeingShown = "";
        }

        public bool IsShowing() => !string.IsNullOrEmpty(UuidBeingShown);

#region Ad callbacks
        
        internal virtual void OnInitialized(bool success) =>  State = success ? AdLoadingState.Initialized : AdLoadingState.Disabled;
        
        public virtual void OnLoading()
        {
            State = AdLoadingState.Loading;
            GenerateUuid();
            LoadingTime.Start();
        }

        public virtual void OnLoadSuccess() => State = AdLoadingState.Loaded;
        
        public virtual void OnLoadFailed() => State = AdLoadingState.Failed;
        
#endregion
    }
}