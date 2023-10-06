using UnityEngine;

namespace Voodoo.Sauce.Internal.Ads
{
    public enum AdLoadingState
    {
        NotInitialized,
        Initialized,
        Loading,
        Loaded, 
        Failed,
        Disabled
    }
    
    // Define an extension method to apply to the enum 'AdLoadingState'.
    public static class Extensions
    {
        public static Color ToColor(this AdLoadingState state)
        {
            Color color;
            
            switch (state) {
                case AdLoadingState.NotInitialized:
                    color = Color.gray;
                    break;
                case AdLoadingState.Initialized:
                    color = Color.blue;
                    break;
                case AdLoadingState.Loading:
                    color = Color.yellow;
                    break;
                case AdLoadingState.Loaded:
                    color = Color.green;
                    break;
                case  AdLoadingState.Failed:
                    color = Color.red;
                    break;
                default:
                    color = Color.gray;
                    break;
            }

            return color;
        }
    }
}